using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Messaging;
using ChatApp.Application.Abstractions.Services;
using ChatApp.Domain.Abstractions;
using ChatApp.Domain.Repositories;

namespace ChatApp.Application.UseCases.Rooms.LeaveRoom;

public class LeaveRoomCommandHandler : ICommandHandler<LeaveRoomCommand>
{
    private readonly IUserContext _userContext;
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChatHub _chatHub;

    public LeaveRoomCommandHandler(IUserContext userContext, IChatRoomRepository chatRoomRepository, IUserRepository userRepository, IUnitOfWork unitOfWork, IChatHub chatHub)
    {
        _userContext = userContext;
        _chatRoomRepository = chatRoomRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _chatHub = chatHub;
    }

    public async Task<Result> Handle(LeaveRoomCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetById(_userContext.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(Error.None);
        }

        var room = await _chatRoomRepository.GetById(request.RoomId, cancellationToken);
        if (room is null)
        {
            return Result.Failure(Error.None);
        }

        room.Leave(user);

        if (room.IsEmpty())
        {
            await _chatRoomRepository.Delete(room, cancellationToken);
        }

        await _unitOfWork.Commit(cancellationToken);

        return Result.Success();
    }
}
