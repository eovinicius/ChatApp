namespace ChatApp.Domain.Entities.Messages;

public sealed class AudioContent : MessageContent
{
    public string Url { get; }
    public int Duration { get; }

    public AudioContent(string url, int duration = 0)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL do áudio não pode ser vazia.");

        Duration = duration;
        Url = url;
    }

    public override MessageContentType Type => MessageContentType.Audio;
}