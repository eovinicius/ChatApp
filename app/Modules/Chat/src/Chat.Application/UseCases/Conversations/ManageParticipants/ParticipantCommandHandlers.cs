using BuildingBlocks.Authentication;
using BuildingBlocks.Clock;
using BuildingBlocks.Messaging;

using Chat.Application.Abstractions.Data;
using Chat.Domain.Conversations;
using Chat.Domain.Repositories;

using Identity.Contracts;

using SharedKernel;

namespace Chat.Application.UseCases.Conversations.ManageParticipants;

// Todas as regras de permissão (admin, dono, 1x1 imutável) vivem no agregado;
// os handlers só carregam, delegam e persistem.
internal abstract class ConversationMutationHandler
{
    protected readonly IConversationRepository ConversationRepository;
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly IUserContext UserContext;

    protected ConversationMutationHandler(
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        ConversationRepository = conversationRepository;
        UnitOfWork = unitOfWork;
        UserContext = userContext;
    }

    protected async Task<Result> Mutate(
        Guid conversationId,
        Func<Conversation, Guid, Result> mutation,
        CancellationToken cancellationToken)
    {
        var conversation = await ConversationRepository.GetByIdWithParticipants(conversationId, cancellationToken);
        if (conversation is null)
            return ConversationErrors.NotFound;

        var result = mutation(conversation, UserContext.UserId);
        if (result.IsFailure)
            return result;

        ConversationRepository.Update(conversation);
        await UnitOfWork.Commit(cancellationToken);

        return Result.Success();
    }
}

internal sealed class AddParticipantsCommandHandler : ConversationMutationHandler, ICommandHandler<AddParticipantsCommand>
{
    private readonly IUserDirectory _userDirectory;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AddParticipantsCommandHandler(
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IUserDirectory userDirectory,
        IDateTimeProvider dateTimeProvider)
        : base(conversationRepository, unitOfWork, userContext)
    {
        _userDirectory = userDirectory;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(AddParticipantsCommand request, CancellationToken cancellationToken)
    {
        var userIds = request.UserIds.Distinct().ToList();

        if (userIds.Count == 0)
            return Result.Success();

        var known = await _userDirectory.GetByIds(userIds, cancellationToken);
        if (known.Count != userIds.Count)
            return UserDirectoryErrors.MemberNotFound;

        var utcNow = _dateTimeProvider.UtcNow;

        return await Mutate(request.ConversationId, (conversation, actorId) =>
        {
            foreach (var userId in userIds)
            {
                var result = conversation.AddParticipant(actorId, userId, utcNow);
                if (result.IsFailure)
                    return result;
            }

            return Result.Success();
        }, cancellationToken);
    }
}

internal sealed class RemoveParticipantCommandHandler : ConversationMutationHandler, ICommandHandler<RemoveParticipantCommand>
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public RemoveParticipantCommandHandler(
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IDateTimeProvider dateTimeProvider)
        : base(conversationRepository, unitOfWork, userContext)
        => _dateTimeProvider = dateTimeProvider;

    public Task<Result> Handle(RemoveParticipantCommand request, CancellationToken cancellationToken)
        => Mutate(
            request.ConversationId,
            (conversation, actorId) => conversation.RemoveParticipant(actorId, request.UserId, _dateTimeProvider.UtcNow),
            cancellationToken);
}

internal sealed class LeaveConversationCommandHandler : ConversationMutationHandler, ICommandHandler<LeaveConversationCommand>
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public LeaveConversationCommandHandler(
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IDateTimeProvider dateTimeProvider)
        : base(conversationRepository, unitOfWork, userContext)
        => _dateTimeProvider = dateTimeProvider;

    public Task<Result> Handle(LeaveConversationCommand request, CancellationToken cancellationToken)
        => Mutate(
            request.ConversationId,
            (conversation, actorId) => conversation.RemoveParticipant(actorId, actorId, _dateTimeProvider.UtcNow),
            cancellationToken);
}

internal sealed class PromoteParticipantCommandHandler : ConversationMutationHandler, ICommandHandler<PromoteParticipantCommand>
{
    public PromoteParticipantCommandHandler(
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
        : base(conversationRepository, unitOfWork, userContext) { }

    public Task<Result> Handle(PromoteParticipantCommand request, CancellationToken cancellationToken)
        => Mutate(
            request.ConversationId,
            (conversation, actorId) => conversation.PromoteToAdmin(actorId, request.UserId),
            cancellationToken);
}

internal sealed class DemoteParticipantCommandHandler : ConversationMutationHandler, ICommandHandler<DemoteParticipantCommand>
{
    public DemoteParticipantCommandHandler(
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
        : base(conversationRepository, unitOfWork, userContext) { }

    public Task<Result> Handle(DemoteParticipantCommand request, CancellationToken cancellationToken)
        => Mutate(
            request.ConversationId,
            (conversation, actorId) => conversation.DemoteAdmin(actorId, request.UserId),
            cancellationToken);
}

internal sealed class RenameConversationCommandHandler : ConversationMutationHandler, ICommandHandler<RenameConversationCommand>
{
    public RenameConversationCommandHandler(
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
        : base(conversationRepository, unitOfWork, userContext) { }

    public Task<Result> Handle(RenameConversationCommand request, CancellationToken cancellationToken)
        => Mutate(
            request.ConversationId,
            (conversation, actorId) => conversation.Rename(actorId, request.Name),
            cancellationToken);
}
