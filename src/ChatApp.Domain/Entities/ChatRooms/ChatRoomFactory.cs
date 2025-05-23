using ChatApp.Domain.Entities.Users;

namespace ChatApp.Domain.Entities.ChatRooms;

public static class ChatRoomFactory
{
    public static ChatRoom CreatePublicRoom(string name, User owner)
    {
        return ChatRoom.Create(name, owner, isPrivate: false);
    }

    public static ChatRoom CreatePrivateRoom(string name, User owner, string password)
    {
        return ChatRoom.Create(name, owner, isPrivate: true, password);
    }
}
