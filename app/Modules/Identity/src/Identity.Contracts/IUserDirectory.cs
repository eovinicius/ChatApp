namespace Identity.Contracts;

// API pública do módulo Identity, consumida por outros módulos (ex.: Chat).
// Nenhum módulo faz join direto em identity.Users — só guarda o UserId e resolve por aqui.
public sealed record UserSummary(Guid Id, string Name, string Username, string? AvatarUrl);

public interface IUserDirectory
{
    // Resolve nomes/avatares em lote — evita N+1 ao hidratar uma página de conversas.
    Task<IReadOnlyList<UserSummary>> GetByIds(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    Task<bool> Exists(Guid userId, CancellationToken cancellationToken = default);
}

// O Chat sabe quando alguém desconecta; o Identity é quem guarda o "visto por último".
public interface IUserPresenceSink
{
    Task TouchLastSeen(Guid userId, DateTime at, CancellationToken cancellationToken = default);
}
