using Chat.Domain.Events;

using SharedKernel;

namespace Chat.Domain.Conversations;

// Agregado raiz que substitui a antiga "sala com senha". O tipo (Direct/Group) governa
// todas as invariantes: um 1x1 é imutável em membros e nome; um grupo tem dono,
// administradores e entra/sai gente.
public sealed class Conversation : AggregateRoot
{
    public const int GroupMaxParticipants = 256;
    public const int DirectParticipants = 2;
    public const int MaxNameLength = 100;

    public ConversationType Type { get; private set; }
    public string? Name { get; private set; }
    public string? AvatarUrl { get; private set; }
    public Guid? OwnerId { get; private set; }

    // Só para Direct: par ordenado de ids. Com índice único, é o que impede
    // duas conversas 1x1 entre as mesmas duas pessoas.
    public string? DirectKey { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime LastActivityAt { get; private set; }
    public int MaxParticipants { get; private set; }

    private readonly List<Participant> _participants = [];
    public IReadOnlyCollection<Participant> Participants => _participants.AsReadOnly();

    private Conversation() { }

    private Conversation(ConversationType type, DateTime utcNow, int maxParticipants)
        : base(Guid.NewGuid())
    {
        Type = type;
        CreatedAt = utcNow;
        LastActivityAt = utcNow;
        MaxParticipants = maxParticipants;
    }

    // Ordena o par para que (A,B) e (B,A) produzam a mesma chave.
    public static string BuildDirectKey(Guid userA, Guid userB)
        => userA.CompareTo(userB) <= 0 ? $"{userA:D}:{userB:D}" : $"{userB:D}:{userA:D}";

    public static Result<Conversation> CreateDirect(Guid userA, Guid userB, DateTime utcNow)
    {
        if (userA == userB)
            return ConversationErrors.DirectWithSelf;

        var conversation = new Conversation(ConversationType.Direct, utcNow, DirectParticipants)
        {
            DirectKey = BuildDirectKey(userA, userB)
        };

        conversation._participants.Add(Participant.Create(conversation.Id, userA, ParticipantRole.Member, utcNow));
        conversation._participants.Add(Participant.Create(conversation.Id, userB, ParticipantRole.Member, utcNow));

        conversation.RaiseDomainEvent(new ConversationCreatedEvent(conversation.Id, ConversationType.Direct, userA));

        return conversation;
    }

    public static Result<Conversation> CreateGroup(string name, Guid ownerId, IReadOnlyCollection<Guid> memberIds, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ConversationErrors.EmptyGroupName;

        var distinctMembers = memberIds.Where(id => id != ownerId).Distinct().ToList();

        if (distinctMembers.Count + 1 > GroupMaxParticipants)
            return ConversationErrors.Full;

        var conversation = new Conversation(ConversationType.Group, utcNow, GroupMaxParticipants)
        {
            Name = name.Trim(),
            OwnerId = ownerId
        };

        conversation._participants.Add(Participant.Create(conversation.Id, ownerId, ParticipantRole.Owner, utcNow));

        foreach (var memberId in distinctMembers)
            conversation._participants.Add(Participant.Create(conversation.Id, memberId, ParticipantRole.Member, utcNow));

        conversation.RaiseDomainEvent(new ConversationCreatedEvent(conversation.Id, ConversationType.Group, ownerId));

        return conversation;
    }

    public Participant? FindParticipant(Guid userId)
        => _participants.FirstOrDefault(p => p.UserId == userId);

    public Participant? FindActiveParticipant(Guid userId)
        => _participants.FirstOrDefault(p => p.UserId == userId && p.IsActive);

    public bool IsActiveParticipant(Guid userId) => FindActiveParticipant(userId) is not null;

    public IReadOnlyList<Guid> ActiveParticipantIds()
        => _participants.Where(p => p.IsActive).Select(p => p.UserId).ToList();

    public Result EnsureCanPost(Guid userId)
        => IsActiveParticipant(userId) ? Result.Success() : ConversationErrors.NotParticipant;

    public Result AddParticipant(Guid actorId, Guid userId, DateTime utcNow)
    {
        if (Type == ConversationType.Direct)
            return ConversationErrors.DirectIsImmutable;

        var actorCheck = EnsureAdmin(actorId);
        if (actorCheck.IsFailure)
            return actorCheck;

        var existing = FindParticipant(userId);

        if (existing is { IsActive: true })
            return ConversationErrors.AlreadyParticipant;

        if (_participants.Count(p => p.IsActive) >= MaxParticipants)
            return ConversationErrors.Full;

        if (existing is not null)
            existing.Rejoin(utcNow);
        else
            _participants.Add(Participant.Create(Id, userId, ParticipantRole.Member, utcNow));

        RaiseDomainEvent(new ParticipantAddedEvent(Id, userId));

        return Result.Success();
    }

    public Result RemoveParticipant(Guid actorId, Guid userId, DateTime utcNow)
    {
        if (Type == ConversationType.Direct)
            return ConversationErrors.DirectIsImmutable;

        var isSelfRemoval = actorId == userId;

        // Sair por conta própria é sempre permitido; remover outra pessoa exige admin.
        if (!isSelfRemoval)
        {
            var actorCheck = EnsureAdmin(actorId);
            if (actorCheck.IsFailure)
                return actorCheck;
        }

        var participant = FindActiveParticipant(userId);
        if (participant is null)
            return ConversationErrors.ParticipantNotFound;

        if (participant.IsOwner)
        {
            // Sem dono o grupo fica sem quem promova/renomeie.
            return isSelfRemoval
                ? ConversationErrors.OwnerMustTransferFirst
                : ConversationErrors.CannotRemoveOwner;
        }

        participant.Leave(utcNow);

        RaiseDomainEvent(new ParticipantRemovedEvent(Id, userId));

        return Result.Success();
    }

    public Result PromoteToAdmin(Guid actorId, Guid userId)
    {
        if (Type == ConversationType.Direct)
            return ConversationErrors.DirectIsImmutable;

        var actorCheck = EnsureAdmin(actorId);
        if (actorCheck.IsFailure)
            return actorCheck;

        var participant = FindActiveParticipant(userId);
        if (participant is null)
            return ConversationErrors.ParticipantNotFound;

        if (!participant.IsAdmin)
            participant.ChangeRole(ParticipantRole.Admin);

        return Result.Success();
    }

    public Result DemoteAdmin(Guid actorId, Guid userId)
    {
        if (Type == ConversationType.Direct)
            return ConversationErrors.DirectIsImmutable;

        // Rebaixar admin é prerrogativa do dono.
        var actorCheck = EnsureOwner(actorId);
        if (actorCheck.IsFailure)
            return actorCheck;

        var participant = FindActiveParticipant(userId);
        if (participant is null)
            return ConversationErrors.ParticipantNotFound;

        if (participant.IsOwner)
            return ConversationErrors.CannotRemoveOwner;

        participant.ChangeRole(ParticipantRole.Member);

        return Result.Success();
    }

    public Result TransferOwnership(Guid actorId, Guid newOwnerId)
    {
        if (Type == ConversationType.Direct)
            return ConversationErrors.DirectIsImmutable;

        var actorCheck = EnsureOwner(actorId);
        if (actorCheck.IsFailure)
            return actorCheck;

        var newOwner = FindActiveParticipant(newOwnerId);
        if (newOwner is null)
            return ConversationErrors.ParticipantNotFound;

        FindActiveParticipant(actorId)?.ChangeRole(ParticipantRole.Admin);
        newOwner.ChangeRole(ParticipantRole.Owner);
        OwnerId = newOwnerId;

        return Result.Success();
    }

    public Result Rename(Guid actorId, string name)
    {
        if (Type == ConversationType.Direct)
            return ConversationErrors.DirectIsImmutable;

        var actorCheck = EnsureAdmin(actorId);
        if (actorCheck.IsFailure)
            return actorCheck;

        if (string.IsNullOrWhiteSpace(name))
            return ConversationErrors.EmptyGroupName;

        Name = name.Trim();

        return Result.Success();
    }

    public Result SetAvatar(Guid actorId, string? avatarUrl)
    {
        if (Type == ConversationType.Direct)
            return ConversationErrors.DirectIsImmutable;

        var actorCheck = EnsureAdmin(actorId);
        if (actorCheck.IsFailure)
            return actorCheck;

        AvatarUrl = avatarUrl;

        return Result.Success();
    }

    // Alimenta a ordenação da lista de conversas sem precisar agregar Messages.
    public void RegisterActivity(DateTime sentAt)
    {
        if (sentAt > LastActivityAt)
            LastActivityAt = sentAt;
    }

    public Result MarkRead(Guid userId, Guid lastMessageId, DateTime lastMessageSentAt)
    {
        var participant = FindActiveParticipant(userId);
        if (participant is null)
            return ConversationErrors.NotParticipant;

        if (participant.MarkRead(lastMessageId, lastMessageSentAt))
            RaiseDomainEvent(new MessagesReadEvent(Id, userId, lastMessageId, lastMessageSentAt));

        return Result.Success();
    }

    public Result MarkDelivered(Guid userId, DateTime lastMessageSentAt)
    {
        var participant = FindActiveParticipant(userId);
        if (participant is null)
            return ConversationErrors.NotParticipant;

        participant.MarkDelivered(lastMessageSentAt);

        return Result.Success();
    }

    private Result EnsureAdmin(Guid actorId)
    {
        var actor = FindActiveParticipant(actorId);

        if (actor is null)
            return ConversationErrors.NotParticipant;

        return actor.IsAdmin ? Result.Success() : ConversationErrors.RequiresAdmin;
    }

    private Result EnsureOwner(Guid actorId)
    {
        var actor = FindActiveParticipant(actorId);

        if (actor is null)
            return ConversationErrors.NotParticipant;

        return actor.IsOwner ? Result.Success() : ConversationErrors.RequiresOwner;
    }
}
