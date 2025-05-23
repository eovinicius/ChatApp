using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Services;
using ChatApp.Application.UseCases.Rooms.LeaveRoom;
using ChatApp.Domain.Entities.Users;
using ChatApp.Domain.Repositories;

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
    public void Deve_remover_usuario_quando_sair_da_sala()
    {
        var user = new User("John Doe");
        _userContextMock.UserId.Returns(user.Id);
        _userRepositoryMock.GetById(user.Id, Arg.Any<CancellationToken>());
        _chatRoomRepositoryMock.GetById(Command.RoomId, Arg.Any<CancellationToken>());
    }

}
