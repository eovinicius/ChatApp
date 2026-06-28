using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Messaging;
using ChatApp.Application.Abstractions.Services;
using ChatApp.Domain.Abstractions;
using ChatApp.Domain.Entities.ChatRooms;
using ChatApp.Domain.Entities.Users;
using ChatApp.Domain.Repositories;

namespace ChatApp.Application.UseCases.Rooms.JoinRoom;

public sealed class JoinRoomCommandHandler : ICommandHandler<JoinRoomCommand>
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChatHub _chatHub;
    private readonly IHashService _hashService;

    public JoinRoomCommandHandler(
        IChatRoomRepository chatRoomRepository,
        IUserContext userContext,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IChatHub chatHub,
        IHashService hashService)
    {
        _chatRoomRepository = chatRoomRepository;
        _userContext = userContext;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _chatHub = chatHub;
        _hashService = hashService;
    }

    public async Task<Result> Handle(JoinRoomCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetById(_userContext.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        var room = await _chatRoomRepository.GetById(request.RoomId, cancellationToken);
        if (room is null)
        {
            return Result.Failure(ChatRoomErrors.NotFound);
        }

        if (room.IsPrivate && (request.Password is null || !_hashService.Compare(request.Password, room.Password)))
        {
            return Result.Failure(ChatRoomErrors.InvalidPassword);
        }

        var joinResult = room.Join(user);
        if (joinResult.IsFailure)
            return joinResult;

        await _chatRoomRepository.Update(room, cancellationToken);

        await _unitOfWork.Commit(cancellationToken);

        await _chatHub.JoinGroup(room.Id.ToString(), user.Id.ToString(), cancellationToken);

        return Result.Success();
    }
}
