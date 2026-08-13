using BuildingBlocks.Authentication;
using BuildingBlocks.Messaging;

using Chat.Application.Abstractions.Data;
using Chat.Domain.Conversations;

using Identity.Contracts;

using SharedKernel;

namespace Chat.Application.UseCases.Conversations.GetMyConversations;

public class GetMyConversationsQueryHandler : IQueryHandler<GetMyConversationsQuery, IReadOnlyList<GetMyConversationsResponse>>
{
    private const int MaxTake = 100;
    private const string DeletedMessagePlaceholder = "Esta mensagem foi apagada";

    private readonly IConversationDao _conversationDao;
    private readonly IUserContext _userContext;
    private readonly IUserDirectory _userDirectory;

    public GetMyConversationsQueryHandler(
        IConversationDao conversationDao,
        IUserContext userContext,
        IUserDirectory userDirectory)
    {
        _conversationDao = conversationDao;
        _userContext = userContext;
        _userDirectory = userDirectory;
    }

    public async Task<Result<IReadOnlyList<GetMyConversationsResponse>>> Handle(
        GetMyConversationsQuery request,
        CancellationToken cancellationToken)
    {
        var take = Math.Clamp(request.Take, 1, MaxTake);

        var items = await _conversationDao.GetForUser(_userContext.UserId, request.Before, take, cancellationToken);

        // Hidratação em lote dos títulos das conversas 1x1.
        var otherUserIds = items
            .Where(item => item.OtherUserId is not null)
            .Select(item => item.OtherUserId!.Value)
            .Distinct()
            .ToList();

        var users = otherUserIds.Count == 0
            ? []
            : await _userDirectory.GetByIds(otherUserIds, cancellationToken);

        var usersById = users.ToDictionary(user => user.Id);

        IReadOnlyList<GetMyConversationsResponse> response = items
            .Select(item => Map(item, usersById))
            .ToList();

        return Result.Success(response);
    }

    private static GetMyConversationsResponse Map(ConversationListItem item, IReadOnlyDictionary<Guid, UserSummary> usersById)
    {
        UserSummary? other = item.OtherUserId is not null && usersById.TryGetValue(item.OtherUserId.Value, out var found)
            ? found
            : null;

        var title = item.Type == ConversationType.Group
            ? item.GroupName ?? string.Empty
            : other?.Name ?? "Usuário desconhecido";

        var avatarUrl = item.Type == ConversationType.Group ? item.GroupAvatarUrl : other?.AvatarUrl;

        ConversationLastMessage? lastMessage = item.LastMessageId is null
            ? null
            : new ConversationLastMessage(
                item.LastMessageId.Value,
                item.LastMessageDeleted ? DeletedMessagePlaceholder : item.LastMessagePreview ?? string.Empty,
                item.LastMessageContentType ?? "text",
                item.LastMessageSenderId ?? Guid.Empty,
                item.LastMessageSentAt ?? item.LastActivityAt,
                item.LastMessageDeleted);

        return new GetMyConversationsResponse(
            item.Id,
            item.Type == ConversationType.Group ? "group" : "direct",
            title,
            avatarUrl,
            item.OtherUserId,
            item.LastActivityAt,
            item.UnreadCount,
            lastMessage);
    }
}
