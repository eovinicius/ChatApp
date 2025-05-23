namespace ChatApp.Domain.Entities.ChatRooms;

public sealed class ChatMessage
{
    public Guid Id { get; private set; }
    public Guid ChatRoom { get; private set; }
    public Guid SenderId { get; private set; }
    public string Content { get; private set; }
    public DateTime SentAt { get; private set; }

    private ChatMessage() { }

    public ChatMessage(Guid chatRoomId, Guid userId, string content)
    {
        Id = Guid.NewGuid();
        ChatRoom = chatRoomId;
        SenderId = userId;
        Content = content;
        SentAt = DateTime.UtcNow;
    }

    public static ChatMessage Create(Guid chatRoomId, Guid userId, string content)
    {
        return new ChatMessage(chatRoomId, userId, content);
    }
}