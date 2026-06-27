using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Services;
using ChatApp.Application.UseCases.Rooms.JoinRoom;
using ChatApp.Domain.Entities.ChatRooms;
using ChatApp.Domain.Entities.Users;
using ChatApp.Domain.Repositories;

using FluentAssertions;

using NSubstitute;

namespace ChatApp.UnitTests.Application.ChatRooms;

public class JoinRoomTests
{

    private static readonly JoinRoomCommand Command = new(Guid.NewGuid(), "123");

    private readonly JoinRoomCommandHanlder _handler;
    private readonly IUserRepository _userRepositoryMock;
    private readonly IChatRoomRepository _chatRoomRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IUserContext _userContextMock;
    private readonly IChatHub _chatHubMock;
    public JoinRoomTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _chatRoomRepositoryMock = Substitute.For<IChatRoomRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _userContextMock = Substitute.For<IUserContext>();
        _chatHubMock = Substitute.For<IChatHub>();

        _handler = new JoinRoomCommandHanlder(
            _chatRoomRepositoryMock,
            _userContextMock,
            _userRepositoryMock,
            _unitOfWorkMock,
            _chatHubMock);
    }
    [Fact]
    public async Task Deveria_adicionar_usuario_quando_entrar_na_sala_publica()
    {
        // Arrange
        var ownerUser = new User("John Doe", "username", "password");
        var newMember = new User("George", "username", "password");
        var room = ChatRoom.Create("sala", ownerUser, false).Value;
        _userContextMock.UserId.Returns(newMember.Id);
        _userRepositoryMock.GetById(newMember.Id, Arg.Any<CancellationToken>()).Returns(newMember);
        _chatRoomRepositoryMock.GetById(Command.RoomId, Arg.Any<CancellationToken>()).Returns(room);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        room.Members.Should().HaveCount(2);
        room.Members.Should().Contain(x => x.UserId == newMember.Id);
        await _unitOfWorkMock.Received(1).Commit(Arg.Any<CancellationToken>());
        await _chatRoomRepositoryMock.Received(1).Update(room, Arg.Any<CancellationToken>());
        await _chatHubMock.Received(1).JoinGroup(room.Id.ToString(), newMember.Id.ToString(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_adicionar_usuario_quando_entrar_na_sala_privada_com_senha_correta()
    {
        var ownerUser = new User("John Doe", "username", "password");
        var newMember = new User("George", "username", "password");
        var room = ChatRoom.Create("sala", ownerUser, true, "123").Value;
        _userContextMock.UserId.Returns(newMember.Id);
        _userRepositoryMock.GetById(newMember.Id, Arg.Any<CancellationToken>()).Returns(newMember);
        _chatRoomRepositoryMock.GetById(Command.RoomId, Arg.Any<CancellationToken>()).Returns(room);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        room.Members.Should().HaveCount(2);
        room.Members.Should().Contain(x => x.UserId == newMember.Id);
        await _unitOfWorkMock.Received(1).Commit(Arg.Any<CancellationToken>());
        await _chatRoomRepositoryMock.Received(1).Update(room, Arg.Any<CancellationToken>());
        await _chatHubMock.Received(1).JoinGroup(room.Id.ToString(), newMember.Id.ToString(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_retornar_erro_quando_usuario_nao_existir()
    {
        // Arrange
        var newMember = new User("John Doe", "username", "password");
        _userContextMock.UserId.Returns(newMember.Id);
        _userRepositoryMock.GetById(newMember.Id, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _unitOfWorkMock.DidNotReceive().Commit(Arg.Any<CancellationToken>());
        _chatRoomRepositoryMock.DidNotReceive().Update(Arg.Any<ChatRoom>(), Arg.Any<CancellationToken>());
        _chatHubMock.DidNotReceive().JoinGroup(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_retornar_erro_quando_sala_nao_existir()
    {
        // Arrange
        var newMember = new User("John Doe", "username", "password");
        _userContextMock.UserId.Returns(newMember.Id);
        _userRepositoryMock.GetById(newMember.Id, Arg.Any<CancellationToken>()).Returns(newMember);
        _chatRoomRepositoryMock.GetById(Command.RoomId, Arg.Any<CancellationToken>()).Returns((ChatRoom?)null);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _unitOfWorkMock.DidNotReceive().Commit(Arg.Any<CancellationToken>());
        _chatRoomRepositoryMock.DidNotReceive().Update(Arg.Any<ChatRoom>(), Arg.Any<CancellationToken>());
        _chatHubMock.DidNotReceive().JoinGroup(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_retornar_erro_quando_senha_incorreta()
    {
        // Arrange
        var newMember = new User("George", "username", "password");
        var room = ChatRoom.Create("sala", new User("John Doe", "username", "password"), true, "1234").Value;
        _userContextMock.UserId.Returns(newMember.Id);
        _userRepositoryMock.GetById(newMember.Id, Arg.Any<CancellationToken>()).Returns(newMember);
        _chatRoomRepositoryMock.GetById(Command.RoomId, Arg.Any<CancellationToken>()).Returns(room);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _unitOfWorkMock.DidNotReceive().Commit(Arg.Any<CancellationToken>());
        _chatRoomRepositoryMock.DidNotReceive().Update(Arg.Any<ChatRoom>(), Arg.Any<CancellationToken>());
        _chatHubMock.DidNotReceive().JoinGroup(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Nao_deveria_permitir_entrada_usuarios_quando_limite_for_atigindo()
    {
        // Arrange
        var ownerUser = new User("John Doe", "username", "password");
        var chatRoom = ChatRoom.Create("full room", ownerUser, false).Value;
        int maxMembers = chatRoom.MaxMembers;

        // Preenche a sala até o limite (owner já está na sala, então adiciona maxMembers - 1)
        for (int i = 0; i < maxMembers - 1; i++)
        {
            var user = new User($"User {i}", "username", "password");
            chatRoom.Join(user);
        }

        var newUser = new User("New User", "newuser", "password");
        _userContextMock.UserId.Returns(newUser.Id);
        _userRepositoryMock.GetById(newUser.Id, Arg.Any<CancellationToken>()).Returns(newUser);
        _chatRoomRepositoryMock.GetById(Command.RoomId, Arg.Any<CancellationToken>()).Returns(chatRoom);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        chatRoom.Members.Should().HaveCount(maxMembers);
        await _unitOfWorkMock.DidNotReceive().Commit(Arg.Any<CancellationToken>());
        await _chatRoomRepositoryMock.DidNotReceive().Update(Arg.Any<ChatRoom>(), Arg.Any<CancellationToken>());
        await _chatHubMock.DidNotReceive().JoinGroup(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
