using Chat.Application.Abstractions.Messaging;

using Chat.Domain.Events;

using Microsoft.Extensions.Logging;

namespace Chat.Application.UseCases.Messages.SendMessage;

public sealed class MessageSentEventHandler : IDomainEventHandler<MessageSentEvent>
{
    private readonly ILogger<MessageSentEventHandler> _logger;

    public MessageSentEventHandler(ILogger<MessageSentEventHandler> logger)
        => _logger = logger;

    public Task Handle(MessageSentEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Mensagem enviada: {MessageId} na sala {ChatRoomId} por {SenderId}",
            notification.MessageId, notification.ChatRoomId, notification.SenderId);

        return Task.CompletedTask;
    }
}
