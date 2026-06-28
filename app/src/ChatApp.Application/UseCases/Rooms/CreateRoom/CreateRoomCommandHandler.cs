using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Messaging;
using ChatApp.Application.Abstractions.Services;
using ChatApp.Domain.Abstractions;
using ChatApp.Domain.Entities.ChatRooms;
using ChatApp.Domain.Entities.Users;
using ChatApp.Domain.Repositories;

namespace ChatApp.Application.UseCases.Rooms.CreateRoom;

public sealed class CreateChatroomCommandHandler : ICommandHandler<CreateChatroomCommand, Guid>
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IChatHub _chatHub;
    private readonly IHashService _hashService;

    public CreateChatroomCommandHandler(
        IChatRoomRepository chatRoomRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IChatHub chatHub,
        IUserRepository userRepository,
        IHashService hashService)
    {
        _chatRoomRepository = chatRoomRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _chatHub = chatHub;
        _userRepository = userRepository;
        _hashService = hashService;
    }

    public async Task<Result<Guid>> Handle(CreateChatroomCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetById(_userContext.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<Guid>(UserErrors.NotFound);
        }

        var hashedPassword = request.Password is not null ? _hashService.Hash(request.Password) : null;

        var roomResult = ChatRoom.Create(request.Name, user, request.IsPrivate, hashedPassword);
        if (roomResult.IsFailure)
            return Result.Failure<Guid>(roomResult.Error);

        var chatRoom = roomResult.Value;

        await _chatRoomRepository.Add(chatRoom, cancellationToken);

        await _unitOfWork.Commit(cancellationToken);

        await _chatHub.JoinGroup(chatRoom.Id.ToString(), user.Name, cancellationToken);

        await _chatHub.SendMessageToGroup(
            chatRoom.Id.ToString(),
            $"{user.Name} has created the room {chatRoom.Name}",
            cancellationToken);

        return chatRoom.Id;
    }
}
