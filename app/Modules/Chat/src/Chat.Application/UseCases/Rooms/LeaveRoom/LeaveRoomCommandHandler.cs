using BuildingBlocks.Messaging;

using Chat.Application.Abstractions.Authentication;
using Chat.Application.Abstractions.Data;
using Chat.Application.Abstractions.Services;
using Chat.Domain.Entities.ChatRooms;
using Chat.Domain.Entities.Users;
using Chat.Domain.Repositories;

using SharedKernel;

namespace Chat.Application.UseCases.Rooms.LeaveRoom;

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
            return UserErrors.NotFound;
        }

        var room = await _chatRoomRepository.GetById(request.RoomId, cancellationToken);
        if (room is null)
        {
            return ChatRoomErrors.NotFound;
        }

        room.Leave(user);

        await _chatRoomRepository.Update(room, cancellationToken);

        if (room.IsEmpty())
        {
            await _chatRoomRepository.Delete(room, cancellationToken);
        }

        await _unitOfWork.Commit(cancellationToken);
        return Result.Success();
    }
}
