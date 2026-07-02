using ChatApp.Application.UseCases.Users.RegisterUser;
using ChatApp.Domain.Events;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using NSubstitute;

namespace ChatApp.UnitTests.Application.Users;

public class UserRegisteredEventHandlerTests
{
    private readonly ILogger<UserRegisteredEventHandler> _loggerMock;
    private readonly UserRegisteredEventHandler _handler;

    public UserRegisteredEventHandlerTests()
    {
        _loggerMock = Substitute.For<ILogger<UserRegisteredEventHandler>>();
        _handler = new UserRegisteredEventHandler(_loggerMock);
    }

    [Fact]
    public async Task Deveria_registrar_log_ao_tratar_evento_de_usuario_registrado()
    {
        // Arrange
        var notification = new UserRegisteredEvent(Guid.NewGuid(), "username");

        // Act
        var act = () => _handler.Handle(notification, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();

        _loggerMock.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains(notification.Username) && o.ToString()!.Contains(notification.UserId.ToString())),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
