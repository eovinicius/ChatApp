using ChatApp.Application.UseCases.Rooms.CreateRoom;
using ChatApp.Domain.Events;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using NSubstitute;

namespace ChatApp.UnitTests.Application.ChatRooms;

public class RoomCreatedEventHandlerTests
{
    private readonly ILogger<RoomCreatedEventHandler> _loggerMock;
    private readonly RoomCreatedEventHandler _handler;

    public RoomCreatedEventHandlerTests()
    {
        _loggerMock = Substitute.For<ILogger<RoomCreatedEventHandler>>();
        _handler = new RoomCreatedEventHandler(_loggerMock);
    }

    [Fact]
    public async Task Deveria_registrar_log_ao_tratar_evento_de_sala_criada()
    {
        // Arrange
        var notification = new RoomCreatedEvent(Guid.NewGuid(), "sala-teste", Guid.NewGuid());

        // Act
        var act = () => _handler.Handle(notification, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();

        _loggerMock.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains(notification.RoomName)
                && o.ToString()!.Contains(notification.RoomId.ToString())
                && o.ToString()!.Contains(notification.OwnerId.ToString())),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
