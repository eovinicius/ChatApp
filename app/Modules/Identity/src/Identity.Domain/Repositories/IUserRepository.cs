using Identity.Domain.Entities.Users;

namespace Identity.Domain.Repositories;

public interface IUserRepository
{
    Task Add(User user, CancellationToken cancellationToken = default);
    Task<User?> GetById(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> GetByUsername(string username, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsername(string username, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> Search(string term, Guid excludeUserId, int take, CancellationToken cancellationToken = default);
    void Update(User user);
}
