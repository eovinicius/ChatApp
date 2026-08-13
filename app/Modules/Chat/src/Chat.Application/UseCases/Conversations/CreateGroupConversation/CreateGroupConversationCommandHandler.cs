using BuildingBlocks.Authentication;
using BuildingBlocks.Clock;
using BuildingBlocks.Messaging;

using Chat.Application.Abstractions.Data;
using Chat.Domain.Conversations;
using Chat.Domain.Repositories;

using Identity.Contracts;

using SharedKernel;

namespace Chat.Application.UseCases.Conversations.CreateGroupConversation;

public class CreateGroupConversationCommandHandler : ICommandHandler<CreateGroupConversationCommand, Guid>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IUserDirectory _userDirectory;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateGroupConversationCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateGroupConversationCommand request, CancellationToken cancellationToken)
    {
        var ownerId = _userContext.UserId;

        var memberIds = request.MemberIds.Where(id => id != ownerId).Distinct().ToList();

        if (memberIds.Count > 0)
        {
            // Uma chamada em lote: nada de N+1 ao validar a lista de convidados.
            var known = await _userDirectory.GetByIds(memberIds, cancellationToken);

            if (known.Count != memberIds.Count)
                return UserDirectoryErrors.MemberNotFound;
        }

        var conversationResult = Conversation.CreateGroup(request.Name, ownerId, memberIds, _dateTimeProvider.UtcNow);
        if (conversationResult.IsFailure)
            return conversationResult.Error;

        var conversation = conversationResult.Value;

        await _conversationRepository.Add(conversation, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);

        return conversation.Id;
    }
}
