using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Clock;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Storage;
using ChatApp.Application.UseCases.Messages.DeleteMessage;
using ChatApp.Domain.Entities.Messages;
using ChatApp.Domain.Entities.Users;
using ChatApp.Domain.Repositories;

using FluentAssertions;

using NSubstitute;
using NSubstitute.ReceivedExtensions;

namespace ChatApp.UnitTests.Application.Messages;

public class DeleteMessageTests
{
    private static readonly DeleteMessageCommand Command = new(
        Guid.NewGuid(),
        Guid.NewGuid()
);

    private readonly DeleteMessageCommandHandler _handler;
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly IChatMessageRepository _chatMessageRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IDateTimeProvider _dateTimeProviderMock;
    private readonly IFileStorageService _fileStorageServiceMock;

    public DeleteMessageTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _userContextMock = Substitute.For<IUserContext>();
        _chatMessageRepositoryMock = Substitute.For<IChatMessageRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        _fileStorageServiceMock = Substitute.For<IFileStorageService>();

        _handler = new DeleteMessageCommandHandler(
            _chatMessageRepositoryMock,
            _unitOfWorkMock,
            _userContextMock,
            _dateTimeProviderMock,
            _userRepositoryMock,
            _fileStorageServiceMock
        );
    }


    [Fact]
    public async Task Deveria_deletar_mensagem_com_sucesso()
    {
        var user = new User("John Doe", "username", "password");
        var roomId = Guid.NewGuid();
        var message = new ChatMessage(roomId, ContentType.Text, "test", user.Id, DateTime.UtcNow);

        _userContextMock.UserId.Returns(user.Id);
        _dateTimeProviderMock.UtcNow.Returns(DateTime.UtcNow);
        _userRepositoryMock.GetById(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _chatMessageRepositoryMock.GetById(message.Id, Arg.Any<CancellationToken>()).Returns(message);

        var command = new DeleteMessageCommand(message.Id, roomId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _unitOfWorkMock.Received(1).Commit(Arg.Any<CancellationToken>());
        _chatMessageRepositoryMock.Received(1).Delete(message, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Usuario_Nao_Existir()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var command = new DeleteMessageCommand(messageId, roomId);

        _userContextMock.UserId.Returns(Guid.NewGuid());
        _userRepositoryMock.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _chatMessageRepositoryMock.DidNotReceive().GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        _chatMessageRepositoryMock.DidNotReceive().Delete(Arg.Any<ChatMessage>(), Arg.Any<CancellationToken>());
        _unitOfWorkMock.DidNotReceive().Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Mensagem_Nao_Existir()
    {
        // Arrange
        var user = new User("John Doe", "username", "password");
        var messageId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var command = new DeleteMessageCommand(messageId, roomId);

        _userContextMock.UserId.Returns(user.Id);
        _userRepositoryMock.GetById(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _chatMessageRepositoryMock.GetById(messageId, Arg.Any<CancellationToken>()).Returns((ChatMessage?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _chatMessageRepositoryMock.DidNotReceive().Delete(Arg.Any<ChatMessage>(), Arg.Any<CancellationToken>());
        _unitOfWorkMock.DidNotReceive().Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Usuario_Nao_For_Remetente()
    {
        // Arrange
        var owner = new User("Owner", "owner", "password");
        var otherUser = new User("Other User", "other", "password");
        var roomId = Guid.NewGuid();
        var message = new ChatMessage(roomId, ContentType.Text, "test", owner.Id, DateTime.UtcNow);
        var command = new DeleteMessageCommand(message.Id, roomId);

        _userContextMock.UserId.Returns(otherUser.Id);
        _dateTimeProviderMock.UtcNow.Returns(DateTime.UtcNow);
        _userRepositoryMock.GetById(otherUser.Id, Arg.Any<CancellationToken>()).Returns(otherUser);
        _chatMessageRepositoryMock.GetById(message.Id, Arg.Any<CancellationToken>()).Returns(message);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _chatMessageRepositoryMock.DidNotReceive().Delete(Arg.Any<ChatMessage>(), Arg.Any<CancellationToken>());
        _unitOfWorkMock.DidNotReceive().Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Tempo_Limite_Expirado()
    {
        // Arrange
        var user = new User("John Doe", "username", "password");
        var roomId = Guid.NewGuid();
        var sentAt = DateTime.UtcNow.AddHours(-25); // Mais de 24 horas
        var message = new ChatMessage(roomId, ContentType.Text, "test", user.Id, sentAt);
        var command = new DeleteMessageCommand(message.Id, roomId);

        _userContextMock.UserId.Returns(user.Id);
        _dateTimeProviderMock.UtcNow.Returns(DateTime.UtcNow);
        _userRepositoryMock.GetById(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _chatMessageRepositoryMock.GetById(message.Id, Arg.Any<CancellationToken>()).Returns(message);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _chatMessageRepositoryMock.DidNotReceive().Delete(Arg.Any<ChatMessage>(), Arg.Any<CancellationToken>());
        _unitOfWorkMock.DidNotReceive().Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Deletar_Arquivo_Do_Storage_Quando_Mensagem_For_Arquivo()
    {
        // Arrange
        var user = new User("John Doe", "username", "password");
        var roomId = Guid.NewGuid();
        var message = new ChatMessage(roomId, ContentType.Image, "file-url", user.Id, DateTime.UtcNow);
        var command = new DeleteMessageCommand(message.Id, roomId);

        _userContextMock.UserId.Returns(user.Id);
        _dateTimeProviderMock.UtcNow.Returns(DateTime.UtcNow);
        _userRepositoryMock.GetById(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _chatMessageRepositoryMock.GetById(message.Id, Arg.Any<CancellationToken>()).Returns(message);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _chatMessageRepositoryMock.Received(1).Delete(message, Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).Commit(Arg.Any<CancellationToken>());
        await _fileStorageServiceMock.Received(1).Delete(message.Content, Arg.Any<CancellationToken>());
    }
}
