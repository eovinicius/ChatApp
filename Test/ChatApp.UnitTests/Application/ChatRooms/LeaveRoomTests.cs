using System.Threading.Tasks;

using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Services;
using ChatApp.Application.UseCases.Rooms.LeaveRoom;
using ChatApp.Domain.Abstractions;
using ChatApp.Domain.Entities.ChatRooms;
using ChatApp.Domain.Entities.Users;
using ChatApp.Domain.Repositories;

using FluentAssertions;

using NSubstitute;

namespace ChatApp.UnitTests.Application.ChatRooms;

public class LeaveRoomTests
{

    private static readonly LeaveRoomCommand Command = new(Guid.NewGuid());

    private readonly LeaveRoomCommandHandler _handler;
    private readonly IUserRepository _userRepositoryMock;
    private readonly IChatRoomRepository _chatRoomRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IUserContext _userContextMock;
    private readonly IChatHub _chatHubMock;
    public LeaveRoomTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _chatRoomRepositoryMock = Substitute.For<IChatRoomRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _userContextMock = Substitute.For<IUserContext>();
        _chatHubMock = Substitute.For<IChatHub>();

        _handler = new LeaveRoomCommandHandler(
            _userContextMock,
            _chatRoomRepositoryMock,
            _userRepositoryMock,
            _unitOfWorkMock,
            _chatHubMock);
    }
    [Fact]
    public async Task Deve_remover_usuario_quando_sair_da_sala()
    {
        // Arrange
        var room = ChatRoom.Create("sala", new User("John Doe", "username", "password"), true, "1234");

        var user = new User("George", "username", "password");
        room.Join(user);

        _userContextMock.UserId.Returns(user.Id);
        _userRepositoryMock.GetById(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _chatRoomRepositoryMock.GetById(Command.RoomId, Arg.Any<CancellationToken>()).Returns(room);

        // Act
        await _handler.Handle(Command, CancellationToken.None);

        // Assert
        room.Members.Should().HaveCount(1);
        room.Members.Should().NotContain(x => x.UserId == user.Id);
        await _chatRoomRepositoryMock.Received(1).Update(room, Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_deletar_sala_quando_ultimo_membro_sair()
    {
        // Arrange
        var ownerMember = new User("John Doe", "username", "password");
        var room = ChatRoom.Create("sala", ownerMember, true, "1234");

        _userContextMock.UserId.Returns(ownerMember.Id);
        _userRepositoryMock.GetById(ownerMember.Id, Arg.Any<CancellationToken>()).Returns(ownerMember);
        _chatRoomRepositoryMock.GetById(Command.RoomId, Arg.Any<CancellationToken>()).Returns(room);

        // Act
        await _handler.Handle(Command, CancellationToken.None);

        // Assert
        room.Members.Should().HaveCount(0);
        await _chatRoomRepositoryMock.Received(1).Delete(room, Arg.Any<CancellationToken>());
        await _chatRoomRepositoryMock.Received(1).Update(room, Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_retornar_erro_quando_usuario_nao_existe()
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

    [Fact]
    public async Task Deve_retornar_erro_quando_sala_nao_existe()
    {
        // Arrange
        var user = new User("John Doe", "username", "password");
        _userContextMock.UserId.Returns(user.Id);
        _userRepositoryMock.GetById(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _chatRoomRepositoryMock.GetById(Command.RoomId, Arg.Any<CancellationToken>()).Returns((ChatRoom?)null);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<Error>();
        await _chatRoomRepositoryMock.DidNotReceive().Add(Arg.Any<ChatRoom>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().Commit(Arg.Any<CancellationToken>());
    }
}
