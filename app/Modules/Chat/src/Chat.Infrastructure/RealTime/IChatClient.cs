using Chat.Application.Abstractions.RealTime;

namespace Chat.Infrastructure.RealTime;

// Contrato tipado do cliente. Um único conjunto de nomes de evento — antes o hub e o
// notificador emitiam nomes diferentes para a mesma coisa.
public interface IChatClient
{
    Task MessageReceived(MessageNotification message);

    Task MessageEdited(MessageNotification message);

    Task MessageDeleted(Guid conversationId, Guid messageId);

    Task MessagesRead(ReadReceiptNotification receipt);

    Task ConversationCreated(ConversationNotification conversation);

    Task ParticipantsChanged(Guid conversationId);

    Task UserTyping(Guid conversationId, Guid userId, bool isTyping);

    Task PresenceChanged(Guid userId, bool isOnline, DateTime? lastSeenAt);
}
