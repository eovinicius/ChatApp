namespace ChatApp.Domain.Entities.Messages;

public sealed record ContentType
{
    public static readonly ContentType Text = new("text");
    public static readonly ContentType Image = new("image");
    public static readonly ContentType Audio = new("audio");
    public static readonly ContentType Video = new("video");

    public string Value { get; }

    private ContentType(string value)
    {
        Value = value.ToLowerInvariant();
    }

    public override string ToString() => Value;

    public static ContentType From(string value) =>
        value.ToLowerInvariant() switch
        {
            "text" => Text,
            "image" => Image,
            "audio" => Audio,
            _ => throw new NotSupportedException($"Tipo de conteúdo '{value}' não suportado.")
        };
}
