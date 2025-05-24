using ChatApp.Domain.Entities.Users;

namespace ChatApp.Application.Abstractions.Authentication;

public interface IAuthenticationService
{
    Task<string?> GenereteToken(User user);
}
