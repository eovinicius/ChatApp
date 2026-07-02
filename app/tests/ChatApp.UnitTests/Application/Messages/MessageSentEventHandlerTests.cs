using ChatApp.Application.UseCases.Messages.SendMessage;
using ChatApp.Domain.Events;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using NSubstitute;

namespace ChatApp.UnitTests.Application.Messages;

public class MessageSentEventHandlerTests
{
    private readonly ILogger<MessageSentEventHandler> _loggerMock;
    private readonly MessageSentEventHandler _handler;

    public MessageSentEventHandlerTests()
    {
        _loggerMock = Substitute.For<ILogger<MessageSentEventHandler>>();
        _handler = new MessageSentEventHandler(_loggerMock);
    }

    [Fact]
    public async Task Deveria_registrar_log_ao_tratar_evento_de_mensagem_enviada()
    {
        // Arrange
        var notification = new MessageSentEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act
        var act = () => _handler.Handle(notification, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();

        _loggerMock.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains(notification.MessageId.ToString())
                && o.ToString()!.Contains(notification.ChatRoomId.ToString())
                && o.ToString()!.Contains(notification.SenderId.ToString())),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
