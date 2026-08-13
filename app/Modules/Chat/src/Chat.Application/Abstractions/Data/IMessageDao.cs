using Chat.Application.UseCases.Messages.GetMessages;

namespace Chat.Application.Abstractions.Data;

public interface IMessageDao
{
    Task<IReadOnlyList<MessageListItem>> GetByConversation(
        Guid conversationId,
        DateTime? before,
        int take,
        CancellationToken cancellationToken = default);
}
