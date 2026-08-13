using Chat.Application.UseCases.Conversations.GetMyConversations;

namespace Chat.Application.Abstractions.Data;

public interface IConversationDao
{
    // Read model da tela inicial. Devolve OtherUserId (e não o nome): quem hidrata
    // é o handler, via Identity.Contracts. Se um dia denormalizarmos os usuários
    // dentro do Chat, só o SQL daqui muda.
    Task<IReadOnlyList<ConversationListItem>> GetForUser(
        Guid userId,
        DateTime? before,
        int take,
        CancellationToken cancellationToken = default);

    // Quem compartilha alguma conversa com este usuário — os destinatários dos
    // eventos de presença.
    Task<IReadOnlyList<Guid>> GetContactIds(Guid userId, CancellationToken cancellationToken = default);
}
