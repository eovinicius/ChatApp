using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Clock;
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
    private readonly IDateTimeProvider _dateTimeProvider;

    public SendMessageCommandHandler(
        IUserRepository userRepository,
        IUserContext userContext,
        IChatRoomRepository chatRoomRepository,
        IChatMessageRepository chatMessageRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _userRepository = userRepository;
        _userContext = userContext;
        _chatRoomRepository = chatRoomRepository;
        _chatMessageRepository = chatMessageRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Guid>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.UserId;

        var user = await _userRepository.GetById(currentUserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<Guid>(Error.NullValue);
        }

        var room = await _chatRoomRepository.GetById(request.RoomId, cancellationToken);
        if (room is null)
        {
            return Result.Failure<Guid>(Error.NullValue);
        }

        if (!room.IsUserInRoom(user))
        {
            return Result.Failure<Guid>(Error.NullValue);
        }

        var chatMessage = ChatMessage.Create(
            room.Id,
            ContentType.From(request.ContentType),
            request.Content,
            user.Id,
            _dateTimeProvider.UtcNow
        );

        await _chatMessageRepository.Add(chatMessage, cancellationToken);

        await _unitOfWork.Commit(cancellationToken);

        return Result.Success(Guid.NewGuid());
    }
}