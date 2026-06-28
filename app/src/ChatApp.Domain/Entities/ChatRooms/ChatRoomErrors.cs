using ChatApp.Domain.Abstractions;

namespace ChatApp.Domain.Entities.ChatRooms;

public static class ChatRoomErrors
{
    public static readonly Error NotFound = new("ChatRoom.NotFound", "Sala não encontrada.");
    public static readonly Error AlreadyMember = new("ChatRoom.AlreadyMember", "O usuário já é membro desta sala.");
    public static readonly Error RoomFull = new("ChatRoom.RoomFull", "A sala atingiu o limite máximo de membros.");
    public static readonly Error InvalidPassword = new("ChatRoom.InvalidPassword", "Senha incorreta para esta sala privada.");
    public static readonly Error NotMember = new("ChatRoom.NotMember", "O usuário não é membro desta sala.");
    public static readonly Error EmptyName = new("ChatRoom.EmptyName", "O nome da sala não pode ser vazio.");
    public static readonly Error PrivateRoomRequiresPassword = new("ChatRoom.PrivateRoomRequiresPassword", "Sala privada deve ter uma senha definida.");
}
