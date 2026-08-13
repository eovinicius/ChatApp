using SharedKernel;

namespace Chat.Domain.Entities.ChatRooms;

public static class ChatRoomErrors
{
    public static readonly Error NotFound = new("ChatRoom.NotFound", "Sala não encontrada.", ErrorType.NotFound);
    public static readonly Error AlreadyMember = new("ChatRoom.AlreadyMember", "O usuário já é membro desta sala.", ErrorType.Conflict);
    public static readonly Error RoomFull = new("ChatRoom.RoomFull", "A sala atingiu o limite máximo de membros.", ErrorType.Conflict);
    public static readonly Error InvalidPassword = new("ChatRoom.InvalidPassword", "Senha incorreta para esta sala privada.", ErrorType.Validation);
    public static readonly Error NotMember = new("ChatRoom.NotMember", "O usuário não é membro desta sala.", ErrorType.Forbidden);
    public static readonly Error EmptyName = new("ChatRoom.EmptyName", "O nome da sala não pode ser vazio.", ErrorType.Validation);
    public static readonly Error PrivateRoomRequiresPassword = new("ChatRoom.PrivateRoomRequiresPassword", "Sala privada deve ter uma senha definida.", ErrorType.Validation);
}
