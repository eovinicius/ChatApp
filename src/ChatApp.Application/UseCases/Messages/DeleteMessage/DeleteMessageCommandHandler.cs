using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Clock;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Messaging;
using ChatApp.Application.Abstractions.Storage;
using ChatApp.Domain.Abstractions;
using ChatApp.Domain.Entities.Messages;
using ChatApp.Domain.Repositories;

namespace ChatApp.Application.UseCases.Messages.DeleteMessage;

public class DeleteMessageCommandHandler : ICommandHandler<DeleteMessageCommand>
{
    private readonly IChatMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUserRepository _userRepository;
    private readonly IFileStorageService _fileStorageService;

    public DeleteMessageCommandHandler(IChatMessageRepository messageRepository, IUnitOfWork unitOfWork, IUserContext userContext, IDateTimeProvider dateTimeProvider, IUserRepository userRepository, IFileStorageService fileStorageService)
    {
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _dateTimeProvider = dateTimeProvider;
        _userRepository = userRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.UserId;

        var user = await _userRepository.GetById(currentUserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(Error.None);
        }

        var message = await _messageRepository.GetById(request.MessageId, cancellationToken);

        if (message is null)
        {
            return Result.Failure(Error.None);
        }

        if (!message.CanBeDeletedBy(currentUserId, request.RoomId, _dateTimeProvider.UtcNow))
        {
            return Result.Failure(Error.None);
        }

        _messageRepository.Delete(message, cancellationToken);

        await _unitOfWork.Commit(cancellationToken);

        if (message.ContentType != ContentType.Text)
        {
            await _fileStorageService.Delete(message.Content, cancellationToken);
        }

        return Result.Success();
    }
}
