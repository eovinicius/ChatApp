using ChatApp.Domain.Entities.ChatRooms;
using ChatApp.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Repositories;

public class ChatRoomRepository : IChatRoomRepository
{
    private readonly ChatAppDbContext _dbContext;
    public ChatRoomRepository(ChatAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Add(ChatRoom chatChatroom, CancellationToken cancellationToken)
    {
        await _dbContext.ChatRooms.AddAsync(chatChatroom, cancellationToken);
    }

    public Task Delete(ChatRoom chatRoom, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ChatRoom?> GetById(Guid chatRoomId, CancellationToken cancellationToken)
    {
        return _dbContext.ChatRooms
            .Include(x => x.Members)
            .FirstOrDefaultAsync(x => x.Id == chatRoomId, cancellationToken);
    }

    public Task Update(ChatRoom chatRoom, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}