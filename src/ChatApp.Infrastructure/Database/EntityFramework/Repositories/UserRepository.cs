using ChatApp.Domain.Entities.Users;
using ChatApp.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Database.EntityFramework.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ChatAppDbContext _dbContext;

    public UserRepository(ChatAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Add(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    public async Task<User?> GetById(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public async Task<User?> GetByUsername(string username, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(x => x.Username == username, cancellationToken: cancellationToken);
    }
}