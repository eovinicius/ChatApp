namespace ChatApp.Domain.Entities.Messages;

public sealed class TextContent : MessageContent
{
    public string Text { get; }

    public TextContent(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Texto não pode ser vazio.");

        Text = text;
    }

    public override MessageContentType Type => MessageContentType.Text;
}
