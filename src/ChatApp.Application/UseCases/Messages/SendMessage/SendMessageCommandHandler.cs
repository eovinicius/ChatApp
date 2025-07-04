using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Messaging;
using ChatApp.Domain.Abstractions;
using ChatApp.Domain.Entities.Messages;
using ChatApp.Domain.Repositories;

namespace ChatApp.Application.UseCases.Messages.SendMessage;

public sealed class SendMessageCommandHandler : ICommandHandler<SendMessageCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SendMessageCommandHandler(IUserRepository userRepository, IUserContext userContext, IChatRoomRepository chatRoomRepository, IChatMessageRepository chatMessageRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _userContext = userContext;
        _chatRoomRepository = chatRoomRepository;
        _chatMessageRepository = chatMessageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetById(_userContext.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<Guid>(Error.NullValue);
        }

        var room = await _chatRoomRepository.GetById(request.RoomId, cancellationToken);
        if (room is null)
        {
            return Result.Failure<Guid>(Error.NullValue);
        }

        if (!room.IsUserInRoom(user.Id))
        {
            return Result.Failure<Guid>(Error.NullValue);
        }

        var message = ChatMessage.Create(room.Id, user.Id, request.Content.Type, request.Content.Data);

        await _chatMessageRepository.Add(message, cancellationToken);

        await _unitOfWork.Commit(cancellationToken);

        return Result.Success(message.Id);
    }
}