using ChatApp.Domain.Abstractions;

namespace ChatApp.Domain.Entities.ChatRooms;

public sealed class ChatRoomUser : Entity
{
    public Guid UserId { get; private set; }
    public Guid ChatRoomId { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public bool IsAdmin { get; private set; }

    public ChatRoomUser(Guid chatRoomId, Guid userId)
    {
        ChatRoomId = chatRoomId;
        UserId = userId;
        JoinedAt = DateTime.UtcNow;
        IsAdmin = false;
    }

    public static ChatRoomUser Create(Guid chatRoomId, Guid userId)
    {
        return new ChatRoomUser(chatRoomId, userId);
    }

    public void PromoteToAdmin()
    {
        IsAdmin = true;
    }
}