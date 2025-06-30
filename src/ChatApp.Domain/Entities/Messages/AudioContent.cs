namespace ChatApp.Domain.Entities.Messages;

public sealed class AudioContent : MessageContent
{
    public string Url { get; }

    public AudioContent(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL do áudio não pode ser vazia.");

        Url = url;
    }

    public override MessageContentType Type => MessageContentType.Audio;
}