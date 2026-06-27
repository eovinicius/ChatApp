using ChatApp.Domain.Entities.Messages;

namespace ChatApp.Domain.Repositories;

public interface IChatMessageRepository
{
    Task Add(ChatMessage chatMessage, CancellationToken cancellationToken);
    Task<ChatMessage?> GetById(Guid messageId, CancellationToken cancellationToken);
    Task Delete(ChatMessage chatMessage, CancellationToken cancellationToken = default);
    Task Update(ChatMessage chatMessage, CancellationToken cancellationToken);
}