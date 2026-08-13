using BuildingBlocks.Authentication;
using BuildingBlocks.Messaging;

using Chat.Application.Abstractions.Data;
using Chat.Domain.Conversations;
using Chat.Domain.Messages;
using Chat.Domain.Repositories;

using SharedKernel;

namespace Chat.Application.UseCases.Conversations.MarkAsRead;

// Avança o cursor de leitura do participante. É o que zera as não-lidas e o que
// dispara o ✓✓ azul do outro lado (via MessagesReadEvent).
public class MarkConversationAsReadCommandHandler : ICommandHandler<MarkConversationAsReadCommand>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public MarkConversationAsReadCommandHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result> Handle(MarkConversationAsReadCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdWithParticipants(request.ConversationId, cancellationToken);
        if (conversation is null)
            return ConversationErrors.NotFound;

        var message = await _messageRepository.GetById(request.LastMessageId, cancellationToken);
        if (message is null)
            return MessageErrors.NotFound;

        if (!message.BelongsTo(request.ConversationId))
            return MessageErrors.WrongConversation;

        // O cursor guarda o SentAt da mensagem, não o instante do ack — é o que faz
        // a contagem de não-lidas fechar com o histórico.
        var result = conversation.MarkRead(_userContext.UserId, message.Id, message.SentAt);
        if (result.IsFailure)
            return result;

        _conversationRepository.Update(conversation);
        await _unitOfWork.Commit(cancellationToken);

        return Result.Success();
    }
}
