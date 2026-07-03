using Chat.Application.Abstractions.Authentication;
using Chat.Application.Abstractions.Clock;
using Chat.Application.Abstractions.Data;
using Chat.Application.Abstractions.Messaging;
using Chat.Domain.Abstractions;
using Chat.Domain.Entities.ChatRooms;
using Chat.Domain.Entities.Messages;
using Chat.Domain.Entities.Users;
using Chat.Domain.Repositories;

namespace Chat.Application.UseCases.Messages.SendMessage;

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
            return Result.Failure<Guid>(UserErrors.NotFound);
        }

        var room = await _chatRoomRepository.GetById(request.RoomId, cancellationToken);
        if (room is null)
        {
            return Result.Failure<Guid>(ChatRoomErrors.NotFound);
        }

        if (!room.IsUserInRoom(user))
        {
            return Result.Failure<Guid>(ChatRoomErrors.NotMember);
        }

        var messageResult = ChatMessage.Create(
            room.Id,
            ContentType.From(request.ContentType),
            request.Content,
            user.Id,
            _dateTimeProvider.UtcNow
        );

        if (messageResult.IsFailure)
            return Result.Failure<Guid>(messageResult.Error);

        var chatMessage = messageResult.Value;

        await _chatMessageRepository.Add(chatMessage, cancellationToken);

        await _unitOfWork.Commit(cancellationToken);

        return Result.Success(chatMessage.Id);
    }
}