namespace Chat.Presentation.Requests;

public sealed class SendMessageRequest
{
    public Guid RoomId { get; set; }
    public string Content { get; set; } = default!;
    public string ContentType { get; set; } = default!;
}

public sealed class EditMessageRequest
{
    public Guid RoomId { get; set; }
    public string Content { get; set; } = default!;
}
