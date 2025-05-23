
using ChatApp.Domain.Entities.ChatRooms;
using ChatApp.Domain.Repositories;

namespace ChatApp.Infrastructure.Repositories;

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
}
