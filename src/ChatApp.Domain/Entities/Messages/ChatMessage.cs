using ChatApp.Domain.Abstractions;

namespace ChatApp.Domain.Entities.Messages;

public sealed class ChatMessage
{
    public Guid Id { get; private set; }
    public Guid ChatRoomId { get; private set; }
    public Guid SenderId { get; private set; }
    public MessageContent Content { get; private set; }
    public DateTime SentAt { get; private set; }
    public bool IsEdited { get; private set; }

    private const int MessageEditTimeLimitInHours = 6;
    private const int MessageDeleteTimeLimitInHours = 24;

    private ChatMessage() { }

    public ChatMessage(Guid chatRoomId, Guid userId, MessageContent content, DateTime sendAt)
    {
        Id = Guid.NewGuid();
        ChatRoomId = chatRoomId;
        SenderId = userId;
        Content = content;
        SentAt = sendAt;
    }

    public static ChatMessage Create(Guid chatRoomId, Guid userId, MessageContent content, DateTime sendAt)
    {
        return new ChatMessage(chatRoomId, userId, content, sendAt);
    }

    public bool CanBeDeletedBy(Guid userId, DateTime utcNow)
    {
        if (SenderId != userId)
            return false;

        if (!IsWithinTimeLimit(utcNow, MessageDeleteTimeLimitInHours))
            return false;

        return true;
    }

    public Result Edit(string newContent, DateTime utcNow)
    {
        if (!IsWithinTimeLimit(utcNow, MessageEditTimeLimitInHours))
            return Result.Failure(Error.NullValue);

        if (!IsTextMessage)
        {
            return Result.Failure(Error.NullValue);
        }

        if (string.IsNullOrWhiteSpace(newContent))
            return Result.Failure(Error.NullValue);

        Content = new TextContent(newContent);
        IsEdited = true;

        return Result.Success();
    }

    private bool IsWithinTimeLimit(DateTime utcNow, int limitInHours)
    {
        return SentAt >= utcNow.AddHours(-limitInHours);
    }

    private bool IsTextMessage => Content.Type == MessageContentType.Text;

    public void SetFilePath(string filePath)
    {
        Content = Content switch
        {
            ImageContent => new ImageContent(filePath),
            AudioContent => new AudioContent(filePath),
            _ => throw new InvalidOperationException("só pode ser usado em mensagens de imagem ou áudio."),
        };
    }
}