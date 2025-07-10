using ChatApp.Domain.Abstractions;

namespace ChatApp.Domain.Entities.Messages;

public class ChatMessage
{
    public Guid Id { get; private set; }
    public Guid ChatRoomId { get; private set; }
    public string Content { get; private set; }
    public Guid SenderId { get; private set; }
    public DateTime SentAt { get; private set; }
    public DateTime EditedAt { get; private set; }
    public ContentType ContentType { get; private set; }

    private const int MessageEditTimeLimitInHours = 6;
    private const int MessageDeleteTimeLimitInHours = 24;

    private ChatMessage() { }

    public ChatMessage(Guid chatRoomId, ContentType contentType, string content, Guid senderId, DateTime sendAt)
    {
        Id = Guid.NewGuid();
        ChatRoomId = chatRoomId;
        Content = content;
        ContentType = contentType;
        SenderId = senderId;
        SentAt = sendAt;
    }

    public static ChatMessage Create(Guid chatRoomId, ContentType contentType, string content, Guid senderId, DateTime sendAt)
    {
        return new ChatMessage(chatRoomId, contentType, content, senderId, sendAt);
    }

    public bool CanBeDeletedBy(Guid userId, DateTime utcNow)
    {
        if (SenderId != userId)
            return false;

        if (!IsWithinTimeLimit(utcNow, MessageDeleteTimeLimitInHours))
            return false;

        return true;
    }

    public Result Edit(string newContent, DateTime utcNow)
    {
        if (!IsWithinTimeLimit(utcNow, MessageEditTimeLimitInHours))
            return Result.Failure(Error.NullValue);

        if (!IsTextMessage)
        {
            return Result.Failure(Error.NullValue);
        }

        if (string.IsNullOrWhiteSpace(newContent))
            return Result.Failure(Error.NullValue);

        Content = newContent;
        EditedAt = utcNow;

        return Result.Success();
    }

    public bool IsEdited => EditedAt > SentAt;

    private bool IsWithinTimeLimit(DateTime utcNow, int limitInHours)
    {
        return SentAt >= utcNow.AddHours(-limitInHours);
    }

    public bool IsTextMessage => ContentType == ContentType.Text;
}