using ChatApp.Application.Abstractions.Messaging;

using ChatApp.Domain.Events;

using Microsoft.Extensions.Logging;

namespace ChatApp.Application.UseCases.Rooms.CreateRoom;

internal sealed class RoomCreatedEventHandler : IDomainEventHandler<RoomCreatedEvent>
{
    private readonly ILogger<RoomCreatedEventHandler> _logger;

    public RoomCreatedEventHandler(ILogger<RoomCreatedEventHandler> logger)
        => _logger = logger;

    public Task Handle(RoomCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sala criada: {RoomName} (Id: {RoomId}, Owner: {OwnerId})",
            notification.RoomName, notification.RoomId, notification.OwnerId);

        return Task.CompletedTask;
    }
}
