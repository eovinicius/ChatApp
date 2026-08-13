using BuildingBlocks.Authentication;
using BuildingBlocks.Clock;
using BuildingBlocks.Messaging;

using Chat.Application.Abstractions.Data;
using Chat.Domain.Messages;
using Chat.Domain.Repositories;

using SharedKernel;

namespace Chat.Application.UseCases.Messages.EditMessage;

public class EditMessageCommandHandler : ICommandHandler<EditMessageCommand>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public EditMessageCommandHandler(
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IDateTimeProvider dateTimeProvider)
    {
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(EditMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _messageRepository.GetById(request.MessageId, cancellationToken);
        if (message is null)
            return MessageErrors.NotFound;

        // A autoria e a janela de edição são invariantes do próprio agregado.
        var editResult = message.Edit(_userContext.UserId, request.Content, _dateTimeProvider.UtcNow);
        if (editResult.IsFailure)
            return editResult;

        _messageRepository.Update(message);
        await _unitOfWork.Commit(cancellationToken);

        return Result.Success();
    }
}
