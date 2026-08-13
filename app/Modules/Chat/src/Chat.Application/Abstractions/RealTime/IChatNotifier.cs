namespace Chat.Application.Abstractions.RealTime;

// Contrato único de tempo real. Substitui o IChatHub antigo, que convivia com um
// segundo conjunto de nomes de evento emitido direto pelo hub.
// Todo envio é endereçado a usuários — não a grupos SignalR.
public interface IChatNotifier
{
    Task MessageReceived(IReadOnlyCollection<Guid> userIds, MessageNotification message, CancellationToken cancellationToken = default);

    Task MessageEdited(IReadOnlyCollection<Guid> userIds, MessageNotification message, CancellationToken cancellationToken = default);

    Task MessageDeleted(IReadOnlyCollection<Guid> userIds, Guid conversationId, Guid messageId, CancellationToken cancellationToken = default);

    Task MessagesRead(IReadOnlyCollection<Guid> userIds, ReadReceiptNotification receipt, CancellationToken cancellationToken = default);

    Task ConversationCreated(IReadOnlyCollection<Guid> userIds, ConversationNotification conversation, CancellationToken cancellationToken = default);

    Task ParticipantsChanged(IReadOnlyCollection<Guid> userIds, Guid conversationId, CancellationToken cancellationToken = default);

    Task TypingChanged(IReadOnlyCollection<Guid> userIds, Guid conversationId, Guid userId, bool isTyping, CancellationToken cancellationToken = default);

    Task PresenceChanged(IReadOnlyCollection<Guid> userIds, Guid userId, bool isOnline, DateTime? lastSeenAt, CancellationToken cancellationToken = default);
}

public sealed record MessageNotification(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string Content,
    string ContentType,
    string? FileName,
    DateTime SentAt,
    DateTime? EditedAt);

public sealed record ReadReceiptNotification(
    Guid ConversationId,
    Guid UserId,
    Guid LastReadMessageId,
    DateTime LastReadMessageSentAt);

public sealed record ConversationNotification(
    Guid Id,
    string Type,
    string? Name);
