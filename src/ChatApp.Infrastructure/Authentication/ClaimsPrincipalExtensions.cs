using System.Security.Claims;

using Microsoft.IdentityModel.JsonWebTokens;

namespace ChatApp.Infrastructure.Authentication;

internal static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        var userId = principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(userId, out var parsedUserId) ?
            parsedUserId :
            throw new ApplicationException("O ID do usuário não está disponível");
    }

    public static string GetIdentityId(this ClaimsPrincipal? principal)
    {
        return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
               throw new ApplicationException("A identidade do usuário não está disponível");
    }
}