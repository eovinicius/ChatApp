using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Clock;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Messaging;
using ChatApp.Domain.Abstractions;
using ChatApp.Domain.Repositories;

namespace ChatApp.Application.UseCases.Messages.EditMessage;

public class EditMessageCommandHandler : ICommandHandler<EditMessageCommand>
{
    private readonly IChatMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUserRepository _userRepository;

    public EditMessageCommandHandler(IChatMessageRepository messageRepository, IUnitOfWork unitOfWork, IUserContext userContext, IUserRepository userRepository, IDateTimeProvider dateTimeProvider)
    {
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _userRepository = userRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(EditMessageCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.UserId;

        var user = await _userRepository.GetById(currentUserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<Guid>(Error.NullValue);
        }

        var message = await _messageRepository.GetById(request.MessageId, cancellationToken);

        if (message is null)
        {
            return Result.Failure(Error.None);
        }

        if (message.SenderId != currentUserId)
        {
            return Result.Failure(Error.NullValue);
        }

        var result = message.Edit(request.Content.Data, _dateTimeProvider.UtcNow);

        if (result.IsFailure)
        {
            return result;
        }

        await _messageRepository.Update(message, cancellationToken);

        await _unitOfWork.Commit(cancellationToken);

        return Result.Success();
    }
}
