using SharedKernel;

namespace Chat.Domain.Messages;

public sealed record ContentType
{
    public static readonly ContentType Text = new("text");
    public static readonly ContentType Image = new("image");
    public static readonly ContentType Audio = new("audio");
    public static readonly ContentType Video = new("video");
    public static readonly ContentType File = new("file");

    public string Value { get; }

    private ContentType(string value) => Value = value.ToLowerInvariant();

    // Devolve Result em vez de lançar: entrada vinda do cliente é falha esperada,
    // não excepcional.
    public static Result<ContentType> From(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "text" => Text,
        "image" => Image,
        "audio" => Audio,
        "video" => Video,
        "file" => File,
        _ => MessageErrors.InvalidContentType
    };

    public bool IsMedia => this != Text;

    public override string ToString() => Value;
}
