using BuildingBlocks.Authentication;
using BuildingBlocks.Messaging;
using BuildingBlocks.Pagination;

using Chat.Application.Abstractions.Data;
using Chat.Domain.Conversations;
using Chat.Domain.Repositories;

using SharedKernel;

namespace Chat.Application.UseCases.Messages.GetMessages;

public class GetMessagesQueryHandler : IQueryHandler<GetMessagesQuery, Page<GetMessagesResponse>>
{
    private const int MaxTake = 100;
    private const string DeletedMessagePlaceholder = "Esta mensagem foi apagada";

    private readonly IMessageDao _messageDao;
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserContext _userContext;

    public GetMessagesQueryHandler(
        IMessageDao messageDao,
        IConversationRepository conversationRepository,
        IUserContext userContext)
    {
        _messageDao = messageDao;
        _conversationRepository = conversationRepository;
        _userContext = userContext;
    }

    public async Task<Result<Page<GetMessagesResponse>>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdWithParticipants(request.ConversationId, cancellationToken);
        if (conversation is null)
            return ConversationErrors.NotFound;

        if (!conversation.IsActiveParticipant(_userContext.UserId))
            return ConversationErrors.NotParticipant;

        var take = Math.Clamp(request.Take, 1, MaxTake);

        // take + 1: o item extra só serve para saber se há próxima página.
        var fetched = await _messageDao.GetByConversation(request.ConversationId, request.Before, take + 1, cancellationToken);

        return Page.From(fetched, take, item => item.SentAt).Map(Map);
    }

    private static GetMessagesResponse Map(MessageListItem item) => new(
        item.Id,
        item.ConversationId,
        item.SenderId,
        item.IsDeleted ? DeletedMessagePlaceholder : item.Content,
        item.ContentType,
        item.IsDeleted ? null : item.FileName,
        item.IsDeleted ? null : item.SizeBytes,
        item.SentAt,
        item.EditedAt,
        item.EditedAt.HasValue && item.EditedAt.Value > item.SentAt,
        item.IsDeleted,
        ResolveStatus(item),
        item.ReadByCount);

    // ✓ enviada · ✓✓ entregue a todos · ✓✓ azul lida por todos.
    private static string ResolveStatus(MessageListItem item)
    {
        if (item.OtherParticipantCount == 0)
            return MessageDeliveryStatus.Read;

        if (item.ReadByCount >= item.OtherParticipantCount)
            return MessageDeliveryStatus.Read;

        if (item.DeliveredToCount >= item.OtherParticipantCount)
            return MessageDeliveryStatus.Delivered;

        return MessageDeliveryStatus.Sent;
    }
}
