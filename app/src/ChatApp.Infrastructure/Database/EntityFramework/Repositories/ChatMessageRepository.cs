using ChatApp.Domain.Entities.Messages;
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

    public Task Delete(ChatMessage chatMessage, CancellationToken cancellationToken = default)
    {
        _dbContext.Messages.Remove(chatMessage);
        return Task.CompletedTask;
    }

    public async Task<ChatMessage?> GetById(Guid messageId, CancellationToken cancellationToken)
    {
        return await _dbContext.Messages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);
    }

    public Task Update(ChatMessage chatMessage, CancellationToken cancellationToken)
    {
        _dbContext.Messages.Update(chatMessage);
        return Task.CompletedTask;
    }
}
