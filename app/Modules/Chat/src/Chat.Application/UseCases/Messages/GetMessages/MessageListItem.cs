namespace Chat.Application.UseCases.Messages.GetMessages;

// Os três contadores derivam dos cursores dos participantes e são o que permite ao
// cliente desenhar ✓ (enviada), ✓✓ (entregue a todos) e ✓✓ azul (lida por todos).
public sealed record MessageListItem(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string Content,
    string ContentType,
    string? FileName,
    long? SizeBytes,
    DateTime SentAt,
    DateTime? EditedAt,
    bool IsDeleted,
    int ReadByCount,
    int DeliveredToCount,
    int OtherParticipantCount);
