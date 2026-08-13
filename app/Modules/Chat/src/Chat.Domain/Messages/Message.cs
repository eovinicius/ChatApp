using Chat.Domain.Events;

using SharedKernel;

namespace Chat.Domain.Messages;

public sealed class Message : AggregateRoot
{
    private const int EditTimeLimitInHours = 1;
    private const int DeleteTimeLimitInHours = 24;

    public Guid ConversationId { get; private set; }
    public Guid SenderId { get; private set; }
    public MessageContent Content { get; private set; }
    public DateTime SentAt { get; private set; }
    public DateTime? EditedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public bool IsEdited => EditedAt.HasValue && EditedAt.Value > SentAt;
    public bool IsDeleted => DeletedAt.HasValue;
    public bool IsTextMessage => Content.Type == ContentType.Text;

    private Message() { }

    private Message(Guid conversationId, Guid senderId, MessageContent content, DateTime sentAt)
        : base(Guid.NewGuid())
    {
        ConversationId = conversationId;
        SenderId = senderId;
        Content = content;
        SentAt = sentAt;
    }

    public static Result<Message> Create(Guid conversationId, Guid senderId, MessageContent content, DateTime sentAt)
    {
        var message = new Message(conversationId, senderId, content, sentAt);
        message.RaiseDomainEvent(new MessageSentEvent(message.Id, conversationId, senderId, sentAt));
        return message;
    }

    public Result Edit(Guid actorId, string newContent, DateTime utcNow)
    {
        if (SenderId != actorId)
            return MessageErrors.Unauthorized;

        if (IsDeleted)
            return MessageErrors.AlreadyDeleted;

        if (!IsTextMessage)
            return MessageErrors.NotTextMessage;

        if (!IsWithinTimeLimit(utcNow, EditTimeLimitInHours))
            return MessageErrors.EditWindowExpired;

        if (string.IsNullOrWhiteSpace(newContent))
            return MessageErrors.EmptyContent;

        Content = Content.WithText(newContent);
        EditedAt = utcNow;

        RaiseDomainEvent(new MessageEditedEvent(Id, ConversationId, utcNow));

        return Result.Success();
    }

    // Soft delete: o "Esta mensagem foi apagada" do WhatsApp. Apagar a linha abriria
    // buracos entre o cursor de leitura e a contagem de não-lidas.
    public Result Delete(Guid actorId, DateTime utcNow)
    {
        if (SenderId != actorId)
            return MessageErrors.Unauthorized;

        if (IsDeleted)
            return MessageErrors.AlreadyDeleted;

        if (!IsWithinTimeLimit(utcNow, DeleteTimeLimitInHours))
            return MessageErrors.DeleteWindowExpired;

        DeletedAt = utcNow;

        RaiseDomainEvent(new MessageDeletedEvent(Id, ConversationId, utcNow));

        return Result.Success();
    }

    public bool BelongsTo(Guid conversationId) => ConversationId == conversationId;

    private bool IsWithinTimeLimit(DateTime utcNow, int limitInHours)
        => SentAt >= utcNow.AddHours(-limitInHours);
}
