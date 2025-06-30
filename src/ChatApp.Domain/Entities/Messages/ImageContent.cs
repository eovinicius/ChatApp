namespace ChatApp.Domain.Entities.Messages;

public sealed class ImageContent : MessageContent
{
    public string Url { get; }

    public ImageContent(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL da imagem não pode ser vazia.");

        Url = url;
    }

    public override MessageContentType Type => MessageContentType.Image;
}
