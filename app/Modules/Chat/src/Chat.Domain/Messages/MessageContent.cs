using SharedKernel;

namespace Chat.Domain.Messages;

// Agrupa a invariante que antes vivia solta: texto tem corpo e nenhuma chave de storage;
// mídia tem URL *e* StorageKey. Guardar a key é o que permite apagar de fato o objeto no
// S3 — antes o handler usava a URL pré-assinada como key e nunca apagava nada.
public sealed record MessageContent
{
    public ContentType Type { get; }
    public string Value { get; }
    public string? StorageKey { get; }
    public string? FileName { get; }
    public long? SizeBytes { get; }

    private MessageContent(ContentType type, string value, string? storageKey, string? fileName, long? sizeBytes)
    {
        Type = type;
        Value = value;
        StorageKey = storageKey;
        FileName = fileName;
        SizeBytes = sizeBytes;
    }

    public static Result<MessageContent> CreateText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return MessageErrors.EmptyContent;

        return new MessageContent(ContentType.Text, text.Trim(), null, null, null);
    }

    public static Result<MessageContent> CreateMedia(ContentType type, string url, string storageKey, string? fileName, long? sizeBytes)
    {
        if (type == ContentType.Text)
            return MessageErrors.InvalidContentType;

        if (string.IsNullOrWhiteSpace(url))
            return MessageErrors.EmptyContent;

        if (string.IsNullOrWhiteSpace(storageKey))
            return MessageErrors.MissingStorageKey;

        return new MessageContent(type, url, storageKey, fileName, sizeBytes);
    }

    internal MessageContent WithText(string text) => new(Type, text.Trim(), StorageKey, FileName, SizeBytes);
}
