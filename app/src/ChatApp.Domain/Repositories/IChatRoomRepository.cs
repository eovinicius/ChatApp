using ChatApp.Domain.Entities.ChatRooms;

namespace ChatApp.Domain.Repositories;

public interface IChatRoomRepository
{
    Task Add(ChatRoom chatChatroom, CancellationToken cancellationToken = default);
    Task<ChatRoom?> GetById(Guid chatRoomId, CancellationToken cancellationToken = default);
    Task Delete(ChatRoom chatRoom, CancellationToken cancellationToken = default);
    Task Update(ChatRoom chatRoom, CancellationToken cancellationToken = default);
}