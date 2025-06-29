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

    public bool CanBeDeletedBy(Guid userId, DateTime utcNow, int timeLimitInHours)
    {
        if (SenderId != userId)
            return false;

        if (SentAt < utcNow.AddHours(-timeLimitInHours))
            return false;

        return true;
    }

    public Result Update(string newContent, Guid userId, DateTime utcNow, int timeLimitInHours = 6)
    {
        if (!CanBeEditBy(userId, utcNow, timeLimitInHours))
        {
            return Result.Failure(Error.None);
        }

        if (string.IsNullOrWhiteSpace(newContent))
        {
            return Result.Failure(Error.None);
        }

        Content = newContent;
        IsEdited = true;

        return Result.Success();
    }

    public bool CanBeEditBy(Guid userId, DateTime utcNow, int timeLimitInHours = 6)
    {
        if (SenderId != userId)
            return false;

        if (SentAt < utcNow.AddHours(-timeLimitInHours))
            return false;

        return true;
    }
}