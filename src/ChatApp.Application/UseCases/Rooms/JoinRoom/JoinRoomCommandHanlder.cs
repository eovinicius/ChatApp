using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Messaging;
using ChatApp.Application.Abstractions.Services;
using ChatApp.Domain.Abstractions;
using ChatApp.Domain.Repositories;

namespace ChatApp.Application.UseCases.Rooms.JoinRoom;

public sealed class JoinRoomCommandHanlder : ICommandHandler<JoinRoomCommand>
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChatHub _chatHub;

    public JoinRoomCommandHanlder(
        IChatRoomRepository chatRoomRepository,
        IUserContext userContext,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IChatHub chatHub)
    {
        _chatRoomRepository = chatRoomRepository;
        _userContext = userContext;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _chatHub = chatHub;
    }

    public async Task<Result> Handle(JoinRoomCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetById(_userContext.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(Error.NullValue);
        }

        var room = await _chatRoomRepository.GetById(request.RoomId, cancellationToken);
        if (room is null)
        {
            return Result.Failure(Error.NullValue);
        }

        if (room.IsPrivate && !room.ValidatePassword(request.Password))
        {
            return Result.Failure(Error.NullValue);
        }

        room.Join(user);

        await _chatRoomRepository.Update(room, cancellationToken);

        await _unitOfWork.Commit(cancellationToken);

        await _chatHub.JoinGroup(room.Id.ToString(), user.Id.ToString(), cancellationToken);

        return Result.Success();
    }
}