using BuildingBlocks.Authentication;
using BuildingBlocks.Messaging;

using Chat.Domain.Conversations;
using Chat.Domain.Repositories;

using Identity.Contracts;

using SharedKernel;

namespace Chat.Application.UseCases.Conversations.GetConversationById;

public class GetConversationByIdQueryHandler : IQueryHandler<GetConversationByIdQuery, ConversationDetailsResponse>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserContext _userContext;
    private readonly IUserDirectory _userDirectory;

    public GetConversationByIdQueryHandler(
        IConversationRepository conversationRepository,
        IUserContext userContext,
        IUserDirectory userDirectory)
    {
        _conversationRepository = conversationRepository;
        _userContext = userContext;
        _userDirectory = userDirectory;
    }

    public async Task<Result<ConversationDetailsResponse>> Handle(GetConversationByIdQuery request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdWithParticipants(request.ConversationId, cancellationToken);
        if (conversation is null)
            return ConversationErrors.NotFound;

        var currentUserId = _userContext.UserId;

        if (!conversation.IsActiveParticipant(currentUserId))
            return ConversationErrors.NotParticipant;

        var active = conversation.Participants.Where(p => p.IsActive).ToList();

        var users = await _userDirectory.GetByIds(active.Select(p => p.UserId).ToList(), cancellationToken);
        var usersById = users.ToDictionary(user => user.Id);

        var participants = active
            .Select(p =>
            {
                usersById.TryGetValue(p.UserId, out var user);

                return new ConversationParticipantResponse(
                    p.UserId,
                    user?.Name ?? "Usuário desconhecido",
                    user?.Username ?? string.Empty,
                    user?.AvatarUrl,
                    p.Role.ToString().ToLowerInvariant(),
                    p.JoinedAt,
                    p.IsMuted);
            })
            .ToList();

        var isGroup = conversation.Type == ConversationType.Group;

        var title = isGroup
            ? conversation.Name ?? string.Empty
            : participants.FirstOrDefault(p => p.UserId != currentUserId)?.Name ?? "Usuário desconhecido";

        var avatarUrl = isGroup
            ? conversation.AvatarUrl
            : participants.FirstOrDefault(p => p.UserId != currentUserId)?.AvatarUrl;

        return new ConversationDetailsResponse(
            conversation.Id,
            isGroup ? "group" : "direct",
            title,
            avatarUrl,
            conversation.OwnerId,
            conversation.CreatedAt,
            conversation.LastActivityAt,
            participants);
    }
}
