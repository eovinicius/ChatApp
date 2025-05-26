using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Messaging;
using ChatApp.Application.Abstractions.Services;
using ChatApp.Domain.Abstractions;
using ChatApp.Domain.Entities.ChatRooms;
using ChatApp.Domain.Repositories;

namespace ChatApp.Application.UseCases.Rooms.CreateRoom;

public sealed class CreateChatroomCommandHandler : ICommandHandler<CreateChatroomCommand, Guid>
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IChatHub _chatHub;

    public CreateChatroomCommandHandler(
        IChatRoomRepository chatRoomRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IChatHub chatHub,
        IUserRepository userRepository)
    {
        _chatRoomRepository = chatRoomRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _chatHub = chatHub;
        _userRepository = userRepository;
    }

    public async Task<Result<Guid>> Handle(CreateChatroomCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetById(_userContext.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<Guid>(Error.NullValue);
        }

        var chatRoom = ChatRoom.Create(request.Name, user, request.IsPrivate, request.Password);

        await _chatRoomRepository.Add(chatRoom, cancellationToken);

        await _unitOfWork.Commit(cancellationToken);

        await _chatHub.JoinGroup(chatRoom.Id.ToString(), user.Name, cancellationToken);

        return chatRoom.Id;
    }
}