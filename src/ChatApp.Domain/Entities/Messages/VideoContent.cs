namespace ChatApp.Domain.Entities.Messages;

public sealed class VideoContent : MessageContent
{
    public string Url { get; }

    public VideoContent(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Texto não pode ser vazio.");

        Url = url;
    }

    public override MessageContentType Type => MessageContentType.Video;
}
