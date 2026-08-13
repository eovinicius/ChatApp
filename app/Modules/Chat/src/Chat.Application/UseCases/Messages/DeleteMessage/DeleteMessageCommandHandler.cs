using BuildingBlocks.Authentication;
using BuildingBlocks.Clock;
using BuildingBlocks.Messaging;

using Chat.Application.Abstractions.Data;
using Chat.Application.Abstractions.Storage;
using Chat.Domain.Messages;
using Chat.Domain.Repositories;

using SharedKernel;

namespace Chat.Application.UseCases.Messages.DeleteMessage;

public class DeleteMessageCommandHandler : ICommandHandler<DeleteMessageCommand>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IFileStorageService _fileStorage;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DeleteMessageCommandHandler(
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IFileStorageService fileStorage,
        IDateTimeProvider dateTimeProvider)
    {
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _fileStorage = fileStorage;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _messageRepository.GetById(request.MessageId, cancellationToken);
        if (message is null)
            return MessageErrors.NotFound;

        var storageKey = message.Content.StorageKey;

        var deleteResult = message.Delete(_userContext.UserId, _dateTimeProvider.UtcNow);
        if (deleteResult.IsFailure)
            return deleteResult;

        _messageRepository.Update(message);
        await _unitOfWork.Commit(cancellationToken);

        // Agora usa a chave real do objeto. Antes passava a URL pré-assinada como key,
        // então o arquivo nunca era removido do bucket.
        if (!string.IsNullOrWhiteSpace(storageKey))
            await _fileStorage.Delete(storageKey, cancellationToken);

        return Result.Success();
    }
}
