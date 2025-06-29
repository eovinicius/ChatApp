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

    public EditMessageCommandHandler(IChatMessageRepository messageRepository, IUnitOfWork unitOfWork, IUserContext userContext)
    {
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result> Handle(EditMessageCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.UserId;

        var message = await _messageRepository.GetById(request.MessageId, cancellationToken);

        if (message is null)
        {
            return Result.Failure(Error.None);
        }

        var result = message.Edit(request.Content, currentUserId, _dateTimeProvider.UtcNow);

        if (result.IsFailure)
        {
            return result;
        }

        await _messageRepository.Update(message, cancellationToken);

        await _unitOfWork.Commit(cancellationToken);

        return Result.Success();
    }
}
