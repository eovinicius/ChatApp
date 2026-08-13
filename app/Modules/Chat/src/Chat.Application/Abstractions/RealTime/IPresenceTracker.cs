namespace Chat.Application.Abstractions.RealTime;

// Presença é estado efêmero de conexão — não vai para o banco.
// Só o "visto por último" é persistido, e no módulo Identity.
public interface IPresenceTracker
{
    // Retorna true quando esta foi a *primeira* conexão do usuário (transição para online).
    Task<bool> Connect(Guid userId, string connectionId);

    // Retorna true quando esta foi a *última* conexão do usuário (transição para offline).
    Task<bool> Disconnect(Guid userId, string connectionId);

    Task<bool> IsOnline(Guid userId);

    Task<IReadOnlyList<Guid>> FilterOnline(IReadOnlyCollection<Guid> userIds);
}
