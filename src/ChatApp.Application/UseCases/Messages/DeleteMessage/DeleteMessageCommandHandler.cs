using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Clock;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Messaging;
using ChatApp.Domain.Abstractions;
using ChatApp.Domain.Repositories;

namespace ChatApp.Application.UseCases.Messages.DeleteMessage;

public class DeleteMessageCommandHandler : ICommandHandler<DeleteMessageCommand>
{
    private const int MessageDeletionTimeLimitInHours = 6;
    private readonly IChatMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DeleteMessageCommandHandler(IChatMessageRepository messageRepository, IUnitOfWork unitOfWork, IUserContext userContext, IDateTimeProvider dateTimeProvider)
    {
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.UserId;

        var message = await _messageRepository.GetById(request.MessageId, cancellationToken);

        if (message is null)
        {
            return Result.Failure(Error.None);
        }

        if (message.ChatRoomId != request.RoomId)
        {
            return Result.Failure(Error.None);
        }

        if (!message.CanBeDeletedBy(currentUserId, _dateTimeProvider.UtcNow, MessageDeletionTimeLimitInHours))
        {
            return Result.Failure(Error.None);
        }

        _messageRepository.Delete(message, cancellationToken);

        await _unitOfWork.Commit(cancellationToken);

        return Result.Success();
    }
}
