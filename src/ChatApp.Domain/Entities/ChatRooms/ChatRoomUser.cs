namespace ChatApp.Domain.Entities.ChatRooms;

public sealed class ChatRoomUser
{
    public Guid? UserId { get; private set; }
    public string? GuestName { get; private set; }
    public Guid ChatRoomId { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public bool IsAdmin { get; private set; }

    public ChatRoomUser(Guid chatRoomId, Guid? userId, string? guestName)
    {
        ChatRoomId = chatRoomId;
        UserId = userId;
        GuestName = guestName;
        JoinedAt = DateTime.UtcNow;
        IsAdmin = false;
    }

    public static ChatRoomUser Create(Guid chatRoomId, Guid userId)
    {
        return new ChatRoomUser(chatRoomId, userId, null);
    }

    public static ChatRoomUser CreateAnonymous(Guid chatRoomId, string guestName)
    {
        return new ChatRoomUser(chatRoomId, null, guestName);
    }

    public void PromoteToAdmin()
    {
        IsAdmin = true;
    }
}