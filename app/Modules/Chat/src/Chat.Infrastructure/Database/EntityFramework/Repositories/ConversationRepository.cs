using Chat.Domain.Conversations;
using Chat.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

namespace Chat.Infrastructure.Database.EntityFramework.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly ChatAppDbContext _dbContext;

    public ConversationRepository(ChatAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Add(Conversation conversation, CancellationToken cancellationToken = default)
    {
        await _dbContext.Conversations.AddAsync(conversation, cancellationToken);
    }

    public async Task<Conversation?> GetById(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);
    }

    public async Task<Conversation?> GetByIdWithParticipants(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Conversations
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);
    }

    public async Task<Conversation?> GetDirectByKey(string directKey, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Conversations
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.DirectKey == directKey, cancellationToken);
    }

    // Se o agregado já está rastreado (o caso normal: foi carregado por este mesmo
    // DbContext), o change tracker cuida de tudo. Chamar DbSet.Update aqui seria pior
    // que redundante: ele marca o grafo inteiro como Modified, e um Participant recém
    // criado — que já nasce com Id preenchido — viraria UPDATE de uma linha inexistente.
    public void Update(Conversation conversation)
    {
        if (_dbContext.Entry(conversation).State == EntityState.Detached)
            _dbContext.Conversations.Update(conversation);
    }

    public void Delete(Conversation conversation) => _dbContext.Conversations.Remove(conversation);
}
