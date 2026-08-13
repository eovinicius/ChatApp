using BuildingBlocks.Authentication;
using BuildingBlocks.Clock;
using BuildingBlocks.Messaging;

using Chat.Application.Abstractions.Data;
using Chat.Domain.Conversations;
using Chat.Domain.Messages;
using Chat.Domain.Repositories;

using SharedKernel;

namespace Chat.Application.UseCases.Messages.SendMessage;

public class SendMessageCommandHandler : ICommandHandler<SendMessageCommand, Guid>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SendMessageCommandHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        IDateTimeProvider dateTimeProvider)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Guid>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var senderId = _userContext.UserId;

        var conversation = await _conversationRepository.GetByIdWithParticipants(request.ConversationId, cancellationToken);
        if (conversation is null)
            return ConversationErrors.NotFound;

        var canPost = conversation.EnsureCanPost(senderId);
        if (canPost.IsFailure)
            return canPost.Error;

        var contentTypeResult = ContentType.From(request.ContentType);
        if (contentTypeResult.IsFailure)
            return contentTypeResult.Error;

        var contentType = contentTypeResult.Value;

        var contentResult = contentType == ContentType.Text
            ? MessageContent.CreateText(request.Content)
            : MessageContent.CreateMedia(contentType, request.Content, request.StorageKey!, request.FileName, request.SizeBytes);

        if (contentResult.IsFailure)
            return contentResult.Error;

        var sentAt = _dateTimeProvider.UtcNow;

        var messageResult = Message.Create(conversation.Id, senderId, contentResult.Value, sentAt);
        if (messageResult.IsFailure)
            return messageResult.Error;

        var message = messageResult.Value;

        // Mantém a lista de conversas ordenada sem precisar agregar Messages.
        conversation.RegisterActivity(sentAt);

        // Quem envia já leu a própria mensagem.
        conversation.MarkRead(senderId, message.Id, sentAt);

        await _messageRepository.Add(message, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);

        return message.Id;
    }
}
