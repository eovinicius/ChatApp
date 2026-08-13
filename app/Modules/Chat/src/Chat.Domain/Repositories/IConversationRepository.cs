using Chat.Domain.Conversations;

namespace Chat.Domain.Repositories;

public interface IConversationRepository
{
    Task Add(Conversation conversation, CancellationToken cancellationToken = default);

    Task<Conversation?> GetById(Guid conversationId, CancellationToken cancellationToken = default);

    // Carrega o agregado completo — qualquer regra de participante precisa dos membros.
    Task<Conversation?> GetByIdWithParticipants(Guid conversationId, CancellationToken cancellationToken = default);

    // O caminho idempotente do 1x1: mesma dupla, mesma conversa.
    Task<Conversation?> GetDirectByKey(string directKey, CancellationToken cancellationToken = default);

    void Update(Conversation conversation);

    void Delete(Conversation conversation);
}
