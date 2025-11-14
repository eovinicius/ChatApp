using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Clock;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.UseCases.Messages.EditMessage;
using ChatApp.Domain.Entities.Messages;
using ChatApp.Domain.Entities.Users;
using ChatApp.Domain.Repositories;

using FluentAssertions;

using NSubstitute;

namespace ChatApp.UnitTests.Application.Messages;

public class EditMessageTests
{
    private readonly EditMessageCommandHandler _handler;
    private readonly IChatMessageRepository _messageRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IUserContext _userContextMock;
    private readonly IDateTimeProvider _dateTimeProviderMock;
    private readonly IUserRepository _userRepositoryMock;

    public EditMessageTests()
    {
        _messageRepositoryMock = Substitute.For<IChatMessageRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _userContextMock = Substitute.For<IUserContext>();
        _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        _userRepositoryMock = Substitute.For<IUserRepository>();

        _handler = new EditMessageCommandHandler(
            _messageRepositoryMock,
            _unitOfWorkMock,
            _userContextMock,
            _userRepositoryMock,
            _dateTimeProviderMock);
    }

    [Fact]
    public async Task Handle_Deve_Editar_Mensagem_Com_Sucesso()
    {
        // Arrange
        var user = new User("John Doe", "username", "password");
        var roomId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var sentAt = DateTime.UtcNow.AddHours(-1);
        var message = new ChatMessage(roomId, ContentType.Text, "Original message", user.Id, sentAt);

        var command = new EditMessageCommand(messageId, new MessageContent("Text", "Edited message"), roomId);

        _userContextMock.UserId.Returns(user.Id);
        _dateTimeProviderMock.UtcNow.Returns(DateTime.UtcNow);
        _userRepositoryMock.GetById(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _messageRepositoryMock.GetById(messageId, Arg.Any<CancellationToken>()).Returns(message);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        message.Content.Should().Be("Edited message");
        message.EditedAt.Should().NotBeNull();

        await _messageRepositoryMock.Received(1).Update(message, Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Deve_Retornar_Erro_Quando_Usuario_Nao_Existir()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var command = new EditMessageCommand(messageId, new MessageContent("Text", "Edited message"), Guid.NewGuid());

        _userContextMock.UserId.Returns(Guid.NewGuid());
        _userRepositoryMock.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        await _messageRepositoryMock.DidNotReceive().GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _messageRepositoryMock.DidNotReceive().Update(Arg.Any<ChatMessage>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Deve_Retornar_Erro_Quando_Mensagem_Nao_Existir()
    {
        // Arrange
        var user = new User("John Doe", "username", "password");
        var messageId = Guid.NewGuid();
        var command = new EditMessageCommand(messageId, new MessageContent("Text", "Edited message"), Guid.NewGuid());

        _userContextMock.UserId.Returns(user.Id);
        _userRepositoryMock.GetById(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _messageRepositoryMock.GetById(messageId, Arg.Any<CancellationToken>()).Returns((ChatMessage?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        await _messageRepositoryMock.DidNotReceive().Update(Arg.Any<ChatMessage>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Deve_Retornar_Erro_Quando_Usuario_Nao_For_Remetente()
    {
        // Arrange
        var owner = new User("Owner", "owner", "password");
        var otherUser = new User("Other User", "other", "password");
        var roomId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var message = new ChatMessage(roomId, ContentType.Text, "Original message", owner.Id, DateTime.UtcNow);

        var command = new EditMessageCommand(messageId, new MessageContent("Text", "Edited message"), roomId);

        _userContextMock.UserId.Returns(otherUser.Id);
        _userRepositoryMock.GetById(otherUser.Id, Arg.Any<CancellationToken>()).Returns(otherUser);
        _messageRepositoryMock.GetById(messageId, Arg.Any<CancellationToken>()).Returns(message);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        await _messageRepositoryMock.DidNotReceive().Update(Arg.Any<ChatMessage>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Deve_Retornar_Erro_Quando_Tempo_Limite_Expirado()
    {
        // Arrange
        var user = new User("John Doe", "username", "password");
        var roomId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var sentAt = DateTime.UtcNow.AddHours(-7); // Mais de 6 horas
        var message = new ChatMessage(roomId, ContentType.Text, "Original message", user.Id, sentAt);

        var command = new EditMessageCommand(messageId, new MessageContent("Text", "Edited message"), roomId);

        _userContextMock.UserId.Returns(user.Id);
        _dateTimeProviderMock.UtcNow.Returns(DateTime.UtcNow);
        _userRepositoryMock.GetById(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _messageRepositoryMock.GetById(messageId, Arg.Any<CancellationToken>()).Returns(message);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        await _messageRepositoryMock.DidNotReceive().Update(Arg.Any<ChatMessage>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().Commit(Arg.Any<CancellationToken>());
    }
}

