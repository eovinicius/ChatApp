namespace Chat.Presentation.Requests;

public sealed class CreateChatRoomRequest
{
    public string RoomName { get; set; } = default!;
    public string? Password { get; set; }
    public bool IsPrivate { get; set; }
}

public sealed class JoinRoomRequest
{
    public string? Password { get; set; }
}
