using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Domain.Entities.Users;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ChatApp.Infrastructure.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IConfiguration _configuration;

    public AuthenticationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string? GenerateToken(User user)
    {
        var handler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            ]),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes("rV0xK+6G8xZJ3m9rTqMev2Yn1w+8WpFlvT5X8NVa1jJU=")),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = handler.CreateToken(tokenDescriptor);
        var tokenString = handler.WriteToken(token);
        return tokenString;
    }
}
