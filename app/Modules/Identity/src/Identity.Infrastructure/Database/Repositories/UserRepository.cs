using Identity.Domain.Entities.Users;
using Identity.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Database.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _dbContext;

    public UserRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Add(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    public async Task<User?> GetById(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public async Task<User?> GetByUsername(string username, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(x => x.Username == username, cancellationToken);
    }

    public async Task<bool> ExistsByUsername(string username, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.AnyAsync(x => x.Username == username, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> Search(string term, Guid excludeUserId, int take, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id != excludeUserId);

        if (!string.IsNullOrWhiteSpace(term))
        {
            var pattern = $"%{term.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Username, pattern) || EF.Functions.ILike(x.Name, pattern));
        }

        return await query
            .OrderBy(x => x.Username)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public void Update(User user)
    {
        _dbContext.Users.Update(user);
    }
}
