using Chat.Application.Abstractions.RealTime;

using Microsoft.AspNetCore.SignalR;

namespace Chat.Infrastructure.RealTime;

// Envia sempre por usuário (Clients.Users), nunca por grupo. Como o NameClaimType é
// NameIdentifier, o IUserIdProvider padrão do SignalR já resolve o UserId — o que
// elimina toda a contabilidade de grupos, sobrevive a reconexão e entrega em todos
// os dispositivos do usuário.
public class SignalRChatNotifier : IChatNotifier
{
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;

    public SignalRChatNotifier(IHubContext<ChatHub, IChatClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task MessageReceived(IReadOnlyCollection<Guid> userIds, MessageNotification message, CancellationToken cancellationToken = default)
        => To(userIds).MessageReceived(message);

    public Task MessageEdited(IReadOnlyCollection<Guid> userIds, MessageNotification message, CancellationToken cancellationToken = default)
        => To(userIds).MessageEdited(message);

    public Task MessageDeleted(IReadOnlyCollection<Guid> userIds, Guid conversationId, Guid messageId, CancellationToken cancellationToken = default)
        => To(userIds).MessageDeleted(conversationId, messageId);

    public Task MessagesRead(IReadOnlyCollection<Guid> userIds, ReadReceiptNotification receipt, CancellationToken cancellationToken = default)
        => To(userIds).MessagesRead(receipt);

    public Task ConversationCreated(IReadOnlyCollection<Guid> userIds, ConversationNotification conversation, CancellationToken cancellationToken = default)
        => To(userIds).ConversationCreated(conversation);

    public Task ParticipantsChanged(IReadOnlyCollection<Guid> userIds, Guid conversationId, CancellationToken cancellationToken = default)
        => To(userIds).ParticipantsChanged(conversationId);

    public Task TypingChanged(IReadOnlyCollection<Guid> userIds, Guid conversationId, Guid userId, bool isTyping, CancellationToken cancellationToken = default)
        => To(userIds).UserTyping(conversationId, userId, isTyping);

    public Task PresenceChanged(IReadOnlyCollection<Guid> userIds, Guid userId, bool isOnline, DateTime? lastSeenAt, CancellationToken cancellationToken = default)
        => To(userIds).PresenceChanged(userId, isOnline, lastSeenAt);

    private IChatClient To(IReadOnlyCollection<Guid> userIds)
        => _hubContext.Clients.Users(userIds.Select(id => id.ToString()).ToList());
}
