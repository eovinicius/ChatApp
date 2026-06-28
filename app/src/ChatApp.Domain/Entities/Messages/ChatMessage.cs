using ChatApp.Domain.Abstractions;
using ChatApp.Domain.Events;

namespace ChatApp.Domain.Entities.Messages;

public class ChatMessage : AggregateRoot
{
    public Guid ChatRoomId { get; private set; }
    public string Content { get; private set; }
    public Guid SenderId { get; private set; }
    public DateTime SentAt { get; private set; }
    public DateTime? EditedAt { get; private set; }
    public ContentType ContentType { get; private set; }

    private const int MessageEditTimeLimitInHours = 1;
    private const int MessageDeleteTimeLimitInHours = 24;

    private ChatMessage() { }

    public ChatMessage(Guid chatRoomId, ContentType contentType, string content, Guid senderId, DateTime sendAt)
        : base(Guid.NewGuid())
    {
        ChatRoomId = chatRoomId;
        Content = content;
        ContentType = contentType;
        SenderId = senderId;
        SentAt = sendAt;
    }

    public static Result<ChatMessage> Create(Guid chatRoomId, ContentType contentType, string content, Guid senderId, DateTime sendAt)
    {
        if (contentType == ContentType.Text && string.IsNullOrWhiteSpace(content))
            return Result.Failure<ChatMessage>(ChatMessageErrors.EmptyContent);

        var message = new ChatMessage(chatRoomId, contentType, content, senderId, sendAt);
        message.RaiseDomainEvent(new MessageSentEvent(message.Id, chatRoomId, senderId));
        return Result.Success(message);
    }

    public bool CanBeDeletedBy(Guid userId, Guid roomId, DateTime utcNow)
    {
        if (SenderId != userId)
            return false;

        if (ChatRoomId != roomId)
            return false;

        if (!IsWithinTimeLimit(utcNow, MessageDeleteTimeLimitInHours))
            return false;

        return true;
    }

    public Result Edit(string newContent, DateTime utcNow)
    {
        if (!IsWithinTimeLimit(utcNow, MessageEditTimeLimitInHours))
            return Result.Failure(ChatMessageErrors.EditWindowExpired);

        if (!IsTextMessage)
            return Result.Failure(ChatMessageErrors.NotTextMessage);

        if (string.IsNullOrWhiteSpace(newContent))
            return Result.Failure(ChatMessageErrors.EmptyContent);

        Content = newContent;
        EditedAt = utcNow;

        return Result.Success();
    }

    public bool IsEdited => EditedAt.HasValue && EditedAt.Value > SentAt;

    private bool IsWithinTimeLimit(DateTime utcNow, int limitInHours)
    {
        return SentAt >= utcNow.AddHours(-limitInHours);
    }

    public bool IsTextMessage => ContentType == ContentType.Text;
}