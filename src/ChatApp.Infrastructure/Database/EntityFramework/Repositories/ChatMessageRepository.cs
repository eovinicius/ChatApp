
using ChatApp.Domain.Entities.ChatRooms;
using ChatApp.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Database.EntityFramework.Repositories;

public class ChatMessageRepository : IChatMessageRepository
{
    private readonly ChatAppDbContext _dbContext;

    public ChatMessageRepository(ChatAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Add(ChatMessage chatMessage, CancellationToken cancellationToken)
    {
        await _dbContext.Messages.AddAsync(chatMessage, cancellationToken);
    }

    public void Delete(ChatMessage chatMessage, CancellationToken cancellationToken)
    {
        _dbContext.Messages.Remove(chatMessage);
    }

    public async Task<ChatMessage?> GetById(Guid messageId, CancellationToken cancellationToken)
    {
        return await _dbContext.Messages.FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);
    }

    public async Task Update(ChatMessage chatMessage, CancellationToken cancellationToken)
    {
        _dbContext.Messages.Update(chatMessage);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
