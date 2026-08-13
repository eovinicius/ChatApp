using System.Security.Claims;

namespace Identity.Infrastructure.Authentication;

internal static class ClaimsPrincipalExtensions
{
    public static bool TryGetUserId(this ClaimsPrincipal? principal, out Guid userId)
    {
        var value = principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out userId);
    }

    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        return principal.TryGetUserId(out var userId)
            ? userId
            : throw new ApplicationException("O ID do usuário não está disponível");
    }
}
