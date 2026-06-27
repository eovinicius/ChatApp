using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Messaging;
using ChatApp.Application.Abstractions.Services;
using ChatApp.Domain.Abstractions;
using ChatApp.Domain.Entities.ChatRooms;
using ChatApp.Domain.Repositories;

namespace ChatApp.Application.UseCases.Rooms.CreateRoomAsAnonymous;

public sealed class CreateRoomAsAnonymousCommandHandler : ICommandHandler<CreateRoomAsAnonymousCommand, Guid>
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChatHub _chatHub;

    public CreateRoomAsAnonymousCommandHandler(IChatRoomRepository chatRoomRepository, IUnitOfWork unitOfWork, IChatHub chatHub)
    {
        _chatRoomRepository = chatRoomRepository;
        _unitOfWork = unitOfWork;
        _chatHub = chatHub;
    }

    public async Task<Result<Guid>> Handle(CreateRoomAsAnonymousCommand request, CancellationToken cancellationToken)
    {
        var room = ChatRoom.CreateAnonymous(request.RoomName, request.GuestName);

        await _chatRoomRepository.Add(room, cancellationToken);

        await _unitOfWork.Commit(cancellationToken);

        await _chatHub.JoinGroup(room.Id.ToString(), request.GuestName, cancellationToken);

        return Result.Success(room.Id);
    }
}