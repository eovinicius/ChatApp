using ChatApp.Domain.Entities.Users;

namespace ChatApp.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetById(Guid userId, CancellationToken cancellationToken);
    Task<User?> GetByUsername(string username);
}