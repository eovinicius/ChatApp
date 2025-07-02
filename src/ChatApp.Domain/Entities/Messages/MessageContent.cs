namespace ChatApp.Domain.Entities.Messages;

public abstract class MessageContent
{
    public abstract MessageContentType Type { get; }

    public static MessageContent Create(string type, string data)
    {
        return type switch
        {
            "text" => new TextContent(data),
            "image" => new ImageContent(data),
            "audio" => new AudioContent(data),
            _ => throw new NotSupportedException($"Tipo '{type}' não suportado.")
        };
    }
}
