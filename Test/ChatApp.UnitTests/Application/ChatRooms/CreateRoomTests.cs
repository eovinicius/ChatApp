using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Services;
using ChatApp.Application.UseCases.Rooms.CreateRoom;
using ChatApp.Domain.Abstractions;
using ChatApp.Domain.Entities.ChatRooms;
using ChatApp.Domain.Entities.Users;
using ChatApp.Domain.Repositories;

using FluentAssertions;

using NSubstitute;

namespace ChatApp.UnitTests.Application.ChatRooms;

public class CreateRoomTests
{
    private static readonly CreateChatroomCommand Command = new("sala", false);

    private readonly CreateChatroomCommandHandler _handler;
    private readonly IUserRepository _userRepositoryMock;
    private readonly IChatRoomRepository _chatRoomRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IUserContext _userContextMock;
    private readonly IChatHub _chatHubMock;
    public CreateRoomTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _chatRoomRepositoryMock = Substitute.For<IChatRoomRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _userContextMock = Substitute.For<IUserContext>();
        _chatHubMock = Substitute.For<IChatHub>();

        _handler = new CreateChatroomCommandHandler(
            _chatRoomRepositoryMock,
            _unitOfWorkMock,
            _userContextMock,
            _chatHubMock,
            _userRepositoryMock);
    }

    [Fact]
    public async Task Handle_deve_criar_sala_publica()
    {
        // Arrange
        var user = new User("John Doe", "username", "password");
        _userContextMock.UserId.Returns(user.Id);
        _userRepositoryMock.GetById(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _chatRoomRepositoryMock.Received(1).Add(Arg.Is<ChatRoom>(x => x.Name == Command.Name && x.IsPrivate == Command.IsPrivate), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_deve_criar_sala_privada_com_senha()
    {
        // Arrange
        var user = new User("John Doe", "username", "password");
        _userContextMock.UserId.Returns(user.Id);
        _userRepositoryMock.GetById(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        var command = new CreateChatroomCommand("sala", true, "123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        await _chatRoomRepositoryMock.Received(1).Add(Arg.Is<ChatRoom>(x => x.Name == command.Name && x.IsPrivate == command.IsPrivate && x.Password == command.Password), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deveria_retornar_erro_quando_usuario_nao_existir()
    {
        // Arrange
        var user = new User("John Doe", "username", "password");
        _userContextMock.UserId.Returns(user.Id);
        _userRepositoryMock.GetById(user.Id, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<Error>();
        await _chatRoomRepositoryMock.DidNotReceive().Add(Arg.Any<ChatRoom>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().Commit(Arg.Any<CancellationToken>());
    }
}
