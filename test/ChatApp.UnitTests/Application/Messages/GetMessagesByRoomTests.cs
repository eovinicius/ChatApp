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
    private readonly ISqlConnectionFactory _sqlConnectionFactoryMock;
    private readonly IUserContext _userContextMock;
    private readonly IUserRepository _userRepositoryMock;
    private readonly IChatRoomRepository _chatRoomRepositoryMock;

    public GetMessagesByRoomTests()
    {
        _sqlConnectionFactoryMock = Substitute.For<ISqlConnectionFactory>();
        _userContextMock = Substitute.For<IUserContext>();
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _chatRoomRepositoryMock = Substitute.For<IChatRoomRepository>();

        _handler = new GetMessagesByRoomQueryHandler(
            _sqlConnectionFactoryMock,
            _userContextMock,
            _userRepositoryMock,
            _chatRoomRepositoryMock);
    }

    [Fact]
    public async Task Handle_Deve_Retornar_Erro_Quando_Usuario_Nao_Existir()
    {
        // Arrange
        _userContextMock.UserId.Returns(Guid.NewGuid());
        _userRepositoryMock.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _handler.Handle(Query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _sqlConnectionFactoryMock.DidNotReceive().CreateConnection();
    }

    [Fact]
    public async Task Handle_Deve_Retornar_Erro_Quando_Sala_Nao_Existir()
    {
        // Arrange
        var user = new User("John Doe", "username", "password");
        _userContextMock.UserId.Returns(user.Id);
        _userRepositoryMock.GetById(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _chatRoomRepositoryMock.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ChatRoom?)null);

        // Act
        var result = await _handler.Handle(Query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _sqlConnectionFactoryMock.DidNotReceive().CreateConnection();
    }

    [Fact]
    public async Task Handle_Deve_Retornar_Erro_Quando_Usuario_Nao_For_Membro_Da_Sala()
    {
        // Arrange
        var owner = new User("Owner", "owner", "password");
        var outsider = new User("Outsider", "outsider", "password");
        var room = ChatRoom.Create("sala", owner, false).Value;

        _userContextMock.UserId.Returns(outsider.Id);
        _userRepositoryMock.GetById(outsider.Id, Arg.Any<CancellationToken>()).Returns(outsider);
        _chatRoomRepositoryMock.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(room);

        // Act
        var result = await _handler.Handle(Query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _sqlConnectionFactoryMock.DidNotReceive().CreateConnection();
    }
}
