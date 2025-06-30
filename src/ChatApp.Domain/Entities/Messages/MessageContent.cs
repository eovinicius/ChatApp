namespace ChatApp.Domain.Entities.Messages;

public abstract class MessageContent
{
    public abstract MessageContentType Type { get; }
}

