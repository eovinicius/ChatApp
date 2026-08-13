using BuildingBlocks.Messaging;

namespace Chat.Application.UseCases.Messages.GetMessages;

public record GetMessagesQuery(Guid ConversationId, DateTime? Before = null, int Take = 30)
    : IQuery<IReadOnlyList<GetMessagesResponse>>;

public sealed record GetMessagesResponse(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string Content,
    string ContentType,
    string? FileName,
    long? SizeBytes,
    DateTime SentAt,
    DateTime? EditedAt,
    bool IsEdited,
    bool IsDeleted,
    string Status,
    int ReadByCount);

public static class MessageDeliveryStatus
{
    public const string Sent = "sent";
    public const string Delivered = "delivered";
    public const string Read = "read";
}
