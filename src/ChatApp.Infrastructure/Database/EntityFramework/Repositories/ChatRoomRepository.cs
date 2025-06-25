using ChatApp.Domain.Entities.ChatRooms;
using ChatApp.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Database.EntityFramework.Repositories;

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

    public async Task Delete(ChatRoom chatRoom, CancellationToken cancellationToken = default)
    {
        _dbContext.ChatRooms.Remove(chatRoom);
        await Task.CompletedTask;
    }

    public Task<ChatRoom?> GetById(Guid chatRoomId, CancellationToken cancellationToken)
    {
        return _dbContext.ChatRooms
            .Include(x => x.Members)
            .FirstOrDefaultAsync(x => x.Id == chatRoomId, cancellationToken);
    }

    public async Task Update(ChatRoom chatRoom, CancellationToken cancellationToken = default)
    {
        _dbContext.ChatRooms.Update(chatRoom);
        await Task.CompletedTask;
    }
}