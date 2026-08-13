using Chat.Domain.Messages;
using Chat.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

namespace Chat.Infrastructure.Database.EntityFramework.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly ChatAppDbContext _dbContext;

    public MessageRepository(ChatAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Add(Message message, CancellationToken cancellationToken = default)
    {
        await _dbContext.Messages.AddAsync(message, cancellationToken);
    }

    public async Task<Message?> GetById(Guid messageId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Messages.FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);
    }

    public void Update(Message message)
    {
        if (_dbContext.Entry(message).State == EntityState.Detached)
            _dbContext.Messages.Update(message);
    }
}
