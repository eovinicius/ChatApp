using BuildingBlocks.Messaging;

using Chat.Application.Abstractions.RealTime;
using Chat.Domain.Conversations;
using Chat.Domain.Events;
using Chat.Domain.Messages;
using Chat.Domain.Repositories;

namespace Chat.Application.UseCases.RealTime;

// O tempo real sai daqui, dos eventos de domínio — e não espalhado pelos handlers de
// comando. Antes o SendMessage persistia sem notificar, e o hub notificava sem persistir.
internal abstract class NotificationHandlerBase
{
    protected readonly IConversationRepository ConversationRepository;
    protected readonly IChatNotifier Notifier;

    protected NotificationHandlerBase(IConversationRepository conversationRepository, IChatNotifier notifier)
    {
        ConversationRepository = conversationRepository;
        Notifier = notifier;
    }

    // Os destinatários são lidos na hora do envio, nunca carregados dentro do evento:
    // a lista pode ter mudado entre a publicação e o consumo.
    protected async Task<IReadOnlyList<Guid>> Recipients(Guid conversationId, CancellationToken cancellationToken)
    {
        var conversation = await ConversationRepository.GetByIdWithParticipants(conversationId, cancellationToken);

        return conversation?.ActiveParticipantIds() ?? [];
    }

    protected static MessageNotification ToNotification(Message message) => new(
        message.Id,
        message.ConversationId,
        message.SenderId,
        message.IsDeleted ? string.Empty : message.Content.Value,
        message.Content.Type.Value,
        message.Content.FileName,
        message.SentAt,
        message.EditedAt);
}

internal sealed class MessageSentNotificationHandler : NotificationHandlerBase, IDomainEventHandler<MessageSentEvent>
{
    private readonly IMessageRepository _messageRepository;

    public MessageSentNotificationHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IChatNotifier notifier)
        : base(conversationRepository, notifier)
        => _messageRepository = messageRepository;

    public async Task Handle(MessageSentEvent notification, CancellationToken cancellationToken)
    {
        var message = await _messageRepository.GetById(notification.MessageId, cancellationToken);
        if (message is null)
            return;

        var recipients = await Recipients(notification.ConversationId, cancellationToken);
        if (recipients.Count == 0)
            return;

        await Notifier.MessageReceived(recipients, ToNotification(message), cancellationToken);
    }
}

internal sealed class MessageEditedNotificationHandler : NotificationHandlerBase, IDomainEventHandler<MessageEditedEvent>
{
    private readonly IMessageRepository _messageRepository;

    public MessageEditedNotificationHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IChatNotifier notifier)
        : base(conversationRepository, notifier)
        => _messageRepository = messageRepository;

    public async Task Handle(MessageEditedEvent notification, CancellationToken cancellationToken)
    {
        var message = await _messageRepository.GetById(notification.MessageId, cancellationToken);
        if (message is null)
            return;

        var recipients = await Recipients(notification.ConversationId, cancellationToken);
        if (recipients.Count == 0)
            return;

        await Notifier.MessageEdited(recipients, ToNotification(message), cancellationToken);
    }
}

internal sealed class MessageDeletedNotificationHandler : NotificationHandlerBase, IDomainEventHandler<MessageDeletedEvent>
{
    public MessageDeletedNotificationHandler(IConversationRepository conversationRepository, IChatNotifier notifier)
        : base(conversationRepository, notifier) { }

    public async Task Handle(MessageDeletedEvent notification, CancellationToken cancellationToken)
    {
        var recipients = await Recipients(notification.ConversationId, cancellationToken);
        if (recipients.Count == 0)
            return;

        await Notifier.MessageDeleted(recipients, notification.ConversationId, notification.MessageId, cancellationToken);
    }
}

internal sealed class MessagesReadNotificationHandler : NotificationHandlerBase, IDomainEventHandler<MessagesReadEvent>
{
    public MessagesReadNotificationHandler(IConversationRepository conversationRepository, IChatNotifier notifier)
        : base(conversationRepository, notifier) { }

    public async Task Handle(MessagesReadEvent notification, CancellationToken cancellationToken)
    {
        var recipients = await Recipients(notification.ConversationId, cancellationToken);

        // Quem leu não precisa ser avisado da própria leitura.
        var others = recipients.Where(id => id != notification.UserId).ToList();
        if (others.Count == 0)
            return;

        var receipt = new ReadReceiptNotification(
            notification.ConversationId,
            notification.UserId,
            notification.LastReadMessageId,
            notification.LastReadMessageSentAt);

        await Notifier.MessagesRead(others, receipt, cancellationToken);
    }
}

internal sealed class ConversationCreatedNotificationHandler : NotificationHandlerBase, IDomainEventHandler<ConversationCreatedEvent>
{
    public ConversationCreatedNotificationHandler(IConversationRepository conversationRepository, IChatNotifier notifier)
        : base(conversationRepository, notifier) { }

    public async Task Handle(ConversationCreatedEvent notification, CancellationToken cancellationToken)
    {
        var conversation = await ConversationRepository.GetByIdWithParticipants(notification.ConversationId, cancellationToken);
        if (conversation is null)
            return;

        var recipients = conversation.ActiveParticipantIds();
        if (recipients.Count == 0)
            return;

        var payload = new ConversationNotification(
            conversation.Id,
            conversation.Type == ConversationType.Group ? "group" : "direct",
            conversation.Name);

        await Notifier.ConversationCreated(recipients, payload, cancellationToken);
    }
}

internal sealed class ParticipantAddedNotificationHandler : NotificationHandlerBase, IDomainEventHandler<ParticipantAddedEvent>
{
    public ParticipantAddedNotificationHandler(IConversationRepository conversationRepository, IChatNotifier notifier)
        : base(conversationRepository, notifier) { }

    public async Task Handle(ParticipantAddedEvent notification, CancellationToken cancellationToken)
    {
        var recipients = await Recipients(notification.ConversationId, cancellationToken);
        if (recipients.Count == 0)
            return;

        await Notifier.ParticipantsChanged(recipients, notification.ConversationId, cancellationToken);
    }
}

internal sealed class ParticipantRemovedNotificationHandler : NotificationHandlerBase, IDomainEventHandler<ParticipantRemovedEvent>
{
    public ParticipantRemovedNotificationHandler(IConversationRepository conversationRepository, IChatNotifier notifier)
        : base(conversationRepository, notifier) { }

    public async Task Handle(ParticipantRemovedEvent notification, CancellationToken cancellationToken)
    {
        var recipients = await Recipients(notification.ConversationId, cancellationToken);

        // Quem saiu também precisa saber que saiu.
        var targets = recipients.Append(notification.UserId).Distinct().ToList();

        await Notifier.ParticipantsChanged(targets, notification.ConversationId, cancellationToken);
    }
}
