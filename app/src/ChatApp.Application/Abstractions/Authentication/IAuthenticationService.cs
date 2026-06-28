using ChatApp.Domain.Entities.Users;

namespace ChatApp.Application.Abstractions.Authentication;

public interface IAuthenticationService
{
    string? GenerateToken(User user);
}
