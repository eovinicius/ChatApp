using BuildingBlocks.Authentication;

using Microsoft.AspNetCore.Http;

namespace Identity.Infrastructure.Authentication;

internal sealed class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId =>
        TryGetUserId(out var userId)
            ? userId
            : throw new ApplicationException("User context is unavailable");

    public bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;

        var principal = _httpContextAccessor.HttpContext?.User;

        return principal is not null && principal.TryGetUserId(out userId);
    }
}
