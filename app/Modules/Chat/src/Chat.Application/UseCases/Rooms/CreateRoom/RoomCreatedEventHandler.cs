using BuildingBlocks.Messaging;

using Chat.Domain.Events;

using Microsoft.Extensions.Logging;

namespace Chat.Application.UseCases.Rooms.CreateRoom;

public sealed class RoomCreatedEventHandler : IDomainEventHandler<RoomCreatedEvent>
{
    private readonly ILogger<RoomCreatedEventHandler> _logger;

    public RoomCreatedEventHandler(ILogger<RoomCreatedEventHandler> logger)
        => _logger = logger;

    public Task Handle(RoomCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{RoomCreatedEvent} - Event processed successfully", nameof(RoomCreatedEvent));

        return Task.CompletedTask;
    }
}
