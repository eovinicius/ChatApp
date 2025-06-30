namespace ChatApp.Domain.Entities.Messages;

public sealed record MessageContentType
{
    public static readonly MessageContentType Text = new("text");
    public static readonly MessageContentType Image = new("image");
    public static readonly MessageContentType Audio = new("audio");

    public string Value { get; }

    private MessageContentType(string value)
    {
        Value = value.ToLowerInvariant();
    }

    public override string ToString() => Value;

    public static MessageContentType From(string value) =>
        value.ToLowerInvariant() switch
        {
            "text" => Text,
            "image" => Image,
            "audio" => Audio,
            _ => throw new NotSupportedException($"Tipo de conteúdo '{value}' não suportado.")
        };
}
