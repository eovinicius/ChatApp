using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Clock;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Storage;
using ChatApp.Application.UseCases.Messages.SendMessage;
using ChatApp.Domain.Entities.ChatRooms;
using ChatApp.Domain.Entities.Messages;
using ChatApp.Domain.Entities.Users;
using ChatApp.Domain.Repositories;

using FluentAssertions;

using NSubstitute;


namespace ChatApp.UnitTests.Application.Messages;

public class SendMessageCommandHandlerTests
{
    private static readonly SendMessageCommand Command = new(
        Guid.NewGuid(),
        new("text", "Hello World"));

    private readonly SendMessageCommandHandler _handler;
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUserContext _userContext;
    private readonly IChatRoomRepository _chatRoomRepositoryMock;
    private readonly IChatMessageRepository _chatMessageRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IFileStorage _fileStorageMock;
    private readonly IDateTimeProvider _dateTimeProviderMock;

    public SendMessageCommandHandlerTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _userContext = Substitute.For<IUserContext>();
        _chatRoomRepositoryMock = Substitute.For<IChatRoomRepository>();
        _chatMessageRepositoryMock = Substitute.For<IChatMessageRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _fileStorageMock = Substitute.For<IFileStorage>();
        _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();

        _handler = new SendMessageCommandHandler(
            _userRepositoryMock,
            _userContext,
            _chatRoomRepositoryMock,
            _chatMessageRepositoryMock,
            _unitOfWorkMock,
            _fileStorageMock,
            _dateTimeProviderMock
        );
    }

    [Fact]
    public async Task Handle_Deve_Enviar_Mensagem_Com_Sucesso()
    {
        // Arrange
        var user = new User("John Doe", "username", "password");
        var room = ChatRoom.Create("sala", user, false);

        _userContext.UserId.Returns(user.Id);
        _dateTimeProviderMock.UtcNow.Returns(DateTime.UtcNow);
        _userRepositoryMock.GetById(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _chatRoomRepositoryMock.GetById(Command.RoomId, Arg.Any<CancellationToken>()).Returns(room);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _chatMessageRepositoryMock.Received(1).Add(Arg.Any<ChatMessage>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Deve_Retornar_Erro_Quando_Usuario_Nao_Existir()
    {
        // Arrange
        _userContext.UserId.Returns(Guid.NewGuid());
        _userRepositoryMock.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert

        result.IsSuccess.Should().BeFalse();
        await _chatMessageRepositoryMock.DidNotReceive().Add(Arg.Any<ChatMessage>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Deve_Retornar_Erro_Quando_Chat_Nao_Existir()
    {
        // Arrange
        var user = new User("John Doe", "username", "password");

        _userContext.UserId.Returns(user.Id);
        _userRepositoryMock.GetById(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _chatRoomRepositoryMock.GetById(Command.RoomId, Arg.Any<CancellationToken>()).Returns((ChatRoom?)null);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert

        result.IsSuccess.Should().BeFalse();
        await _chatMessageRepositoryMock.DidNotReceive().Add(Arg.Any<ChatMessage>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Deve_Retornar_Erro_Quando_Usuario_Nao_Estiver_Na_Sala()
    {
        // Arrange
        var user = new User("George", "username", "password");
        var room = ChatRoom.Create("sala", new User("John Doe", "username", "password"), false);

        _userContext.UserId.Returns(user.Id);
        _userRepositoryMock.GetById(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _chatRoomRepositoryMock.GetById(Command.RoomId, Arg.Any<CancellationToken>()).Returns(room);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        await _chatMessageRepositoryMock.DidNotReceive().Add(Arg.Any<ChatMessage>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().Commit(Arg.Any<CancellationToken>());
    }
}
