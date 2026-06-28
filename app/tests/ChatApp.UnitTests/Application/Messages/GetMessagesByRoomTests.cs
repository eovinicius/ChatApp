using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.UseCases.Messages.GetMessagesByRoom;
using ChatApp.Domain.Entities.ChatRooms;
using ChatApp.Domain.Entities.Users;
using ChatApp.Domain.Repositories;

using FluentAssertions;

using NSubstitute;

namespace ChatApp.UnitTests.Application.Messages;

public class GetMessagesByRoomTests
{
    private static readonly GetMessagesByRoomQuery Query = new(Guid.NewGuid(), null, 10);

    private readonly GetMessagesByRoomQueryHandler _handler;
    private readonly IMessageDao _messageDaoMock;
    private readonly IUserContext _userContextMock;
    private readonly IUserRepository _userRepositoryMock;
    private readonly IChatRoomRepository _chatRoomRepositoryMock;

    public GetMessagesByRoomTests()
    {
        _messageDaoMock = Substitute.For<IMessageDao>();
        _userContextMock = Substitute.For<IUserContext>();
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _chatRoomRepositoryMock = Substitute.For<IChatRoomRepository>();

        _handler = new GetMessagesByRoomQueryHandler(
            _messageDaoMock,
            _userContextMock,
            _userRepositoryMock,
            _chatRoomRepositoryMock);
    }

    [Fact]
    public async Task Deveria_retornar_erro_quando_usuario_nao_existir()
    {
        // Arrange
        _userContextMock.UserId.Returns(Guid.NewGuid());
        _userRepositoryMock.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _handler.Handle(Query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _messageDaoMock.DidNotReceive().GetByRoom(Arg.Any<Guid>(), Arg.Any<DateTime?>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deveria_retornar_erro_quando_sala_nao_existir()
    {
        // Arrange
        var user = User.Create("John Doe", "username", "password").Value;
        _userContextMock.UserId.Returns(user.Id);
        _userRepositoryMock.GetById(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _chatRoomRepositoryMock.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ChatRoom?)null);

        // Act
        var result = await _handler.Handle(Query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _messageDaoMock.DidNotReceive().GetByRoom(Arg.Any<Guid>(), Arg.Any<DateTime?>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deveria_retornar_erro_quando_usuario_nao_for_membro_da_sala()
    {
        // Arrange
        var owner = User.Create("Owner", "owner", "password").Value;
        var outsider = User.Create("Outsider", "outsider", "password").Value;
        var room = ChatRoom.Create("sala", owner, false).Value;

        _userContextMock.UserId.Returns(outsider.Id);
        _userRepositoryMock.GetById(outsider.Id, Arg.Any<CancellationToken>()).Returns(outsider);
        _chatRoomRepositoryMock.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(room);

        // Act
        var result = await _handler.Handle(Query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _messageDaoMock.DidNotReceive().GetByRoom(Arg.Any<Guid>(), Arg.Any<DateTime?>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}
