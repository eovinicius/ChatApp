using ChatApp.Domain.Entities.ChatRooms;

namespace ChatApp.Domain.Repositories;

public interface IChatMessageRepository
{
    Task Add(ChatMessage chatMessage, CancellationToken cancellationToken);
    Task<ChatMessage?> GetById(Guid messageId, CancellationToken cancellationToken);
    void Delete(ChatMessage chatMessage, CancellationToken cancellationToken);
    Task Update(ChatMessage chatMessage, CancellationToken cancellationToken);
}