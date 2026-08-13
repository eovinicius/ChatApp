using Identity.Contracts;
using Identity.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Contracts;

// Implementação da API pública do módulo. É o único caminho pelo qual outro
// módulo enxerga dados de usuário — ninguém faz join em identity.Users.
internal sealed class UserDirectory : IUserDirectory, IUserPresenceSink
{
    private readonly IdentityDbContext _dbContext;

    public UserDirectory(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<UserSummary>> GetByIds(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return [];

        var distinctIds = ids.Distinct().ToArray();

        return await _dbContext.Users
            .AsNoTracking()
            .Where(user => distinctIds.Contains(user.Id))
            .Select(user => new UserSummary(user.Id, user.Name, user.Username, user.AvatarUrl))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> Exists(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.AnyAsync(user => user.Id == userId, cancellationToken);
    }

    public async Task TouchLastSeen(Guid userId, DateTime at, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return;

        user.TouchLastSeen(at);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
