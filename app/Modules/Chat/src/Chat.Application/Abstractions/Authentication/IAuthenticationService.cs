using Chat.Domain.Entities.Users;

namespace Chat.Application.Abstractions.Authentication;

public interface IAuthenticationService
{
    string? GenerateToken(User user);
}
