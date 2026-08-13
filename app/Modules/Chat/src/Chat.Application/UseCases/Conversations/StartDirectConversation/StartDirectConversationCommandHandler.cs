using BuildingBlocks.Authentication;
using BuildingBlocks.Clock;
using BuildingBlocks.Messaging;

using Chat.Application.Abstractions.Data;
using Chat.Domain.Conversations;
using Chat.Domain.Repositories;

using Identity.Contracts;

using SharedKernel;

namespace Chat.Application.UseCases.Conversations.StartDirectConversation;

// Idempotente: abrir a conversa com a mesma pessoa duas vezes devolve o mesmo id.
// A chave determinística + índice único é o que garante isso mesmo sob concorrência.
public class StartDirectConversationCommandHandler : ICommandHandler<StartDirectConversationCommand, Guid>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IUserDirectory _userDirectory;
    private readonly IDateTimeProvider _dateTimeProvider;

    public StartDirectConversationCommandHandler(
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IUserDirectory userDirectory,
        IDateTimeProvider dateTimeProvider)
    {
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _userDirectory = userDirectory;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Guid>> Handle(StartDirectConversationCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.UserId;

        if (currentUserId == request.TargetUserId)
            return ConversationErrors.DirectWithSelf;

        if (!await _userDirectory.Exists(request.TargetUserId, cancellationToken))
            return UserDirectoryErrors.TargetNotFound;

        var directKey = Conversation.BuildDirectKey(currentUserId, request.TargetUserId);

        var existing = await _conversationRepository.GetDirectByKey(directKey, cancellationToken);
        if (existing is not null)
            return existing.Id;

        var conversationResult = Conversation.CreateDirect(currentUserId, request.TargetUserId, _dateTimeProvider.UtcNow);
        if (conversationResult.IsFailure)
            return conversationResult.Error;

        var conversation = conversationResult.Value;

        await _conversationRepository.Add(conversation, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);

        return conversation.Id;
    }
}
