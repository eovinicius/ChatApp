using ChatApp.Application.Abstractions.Messaging;

using ChatApp.Domain.Events;

using Microsoft.Extensions.Logging;

namespace ChatApp.Application.UseCases.Messages.SendMessage;

internal sealed class MessageSentEventHandler : IDomainEventHandler<MessageSentEvent>
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
