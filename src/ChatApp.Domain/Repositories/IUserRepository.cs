using ChatApp.Domain.Entities.Users;

namespace ChatApp.Domain.Repositories;

public interface IUserRepository
{
    Task Add(User user, CancellationToken cancellationToken = default);
    Task<User?> GetById(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> GetByUsername(string username, CancellationToken cancellationToken = default);
}