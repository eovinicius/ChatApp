using Identity.Domain.Entities.Users;

namespace Identity.Application.Abstractions;

public interface IAuthenticationService
{
    string? GenerateToken(User user);
}
