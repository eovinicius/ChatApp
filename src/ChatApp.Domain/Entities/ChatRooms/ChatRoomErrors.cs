using ChatApp.Domain.Abstractions;

namespace ChatApp.Domain.Entities.ChatRooms;

public static class ChatRoomErrors
{
    public static readonly Error AlreadyMember = new("ChatRoom.AlreadyMember", "O usuário já é membro desta sala.");
    public static readonly Error RoomFull = new("ChatRoom.RoomFull", "A sala atingiu o limite máximo de membros.");
}
