namespace ChatApp.Domain.Entities.Messages;

public static class MessageContentFactory
{
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