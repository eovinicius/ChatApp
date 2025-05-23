using ChatApp.Domain.Entities.ChatRooms;

namespace ChatApp.Domain.Repositories;

public interface IChatMessageRepository
{
    Task Add(ChatMessage chatMessage, CancellationToken cancellationToken);
}