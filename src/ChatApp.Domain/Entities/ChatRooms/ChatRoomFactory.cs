using ChatApp.Domain.Abstractions;
using ChatApp.Domain.Entities.Users;

namespace ChatApp.Domain.Entities.ChatRooms;

public static class ChatRoomFactory
{
    public static Result<ChatRoom> CreatePublicRoom(string name, User owner)
    {
        return ChatRoom.Create(name, owner, isPrivate: false);
    }

    public static Result<ChatRoom> CreatePrivateRoom(string name, User owner, string password)
    {
        return ChatRoom.Create(name, owner, isPrivate: true, password);
    }
}
