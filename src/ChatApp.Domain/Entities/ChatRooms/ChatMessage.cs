using ChatApp.Domain.Abstractions;

namespace ChatApp.Domain.Entities.ChatRooms;

public sealed class ChatMessage
{
    public Guid Id { get; private set; }
    public Guid ChatRoomId { get; private set; }
    public Guid SenderId { get; private set; }
    public string Content { get; private set; }
    public DateTime SentAt { get; private set; }
    public bool IsEdited { get; private set; }

    private const int MessageEditTimeLimitInHours = 6;
    private const int MessageDeleteTimeLimitInHours = 24;

    private ChatMessage() { }

    public ChatMessage(Guid chatRoomId, Guid userId, string content)
    {
        Id = Guid.NewGuid();
        ChatRoomId = chatRoomId;
        SenderId = userId;
        Content = content;
        SentAt = DateTime.UtcNow;
    }

    public static ChatMessage Create(Guid chatRoomId, Guid userId, string content)
    {
        return new ChatMessage(chatRoomId, userId, content);
    }

    public bool CanBeDeletedBy(Guid userId, DateTime utcNow)
    {
        if (SenderId != userId)
            return false;

        if (!IsWithinTimeLimit(utcNow, MessageDeleteTimeLimitInHours))
            return false;

        return true;
    }

    public Result Edit(string newContent, Guid userId, DateTime utcNow)
    {
        if (SenderId != userId)
            return Result.Failure(Error.None);

        if (!IsWithinTimeLimit(utcNow, MessageEditTimeLimitInHours))
            return Result.Failure(Error.None);

        if (string.IsNullOrWhiteSpace(newContent))
            return Result.Failure(Error.None);

        Content = newContent;
        IsEdited = true;

        return Result.Success();
    }

    private bool IsWithinTimeLimit(DateTime utcNow, int limitInHours)
    {
        return SentAt >= utcNow.AddHours(-limitInHours);
    }
}