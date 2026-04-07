using System.Security.Claims;

namespace AuthService.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);

        if (claim == null)
            throw new UnauthorizedAccessException("UserId claim missing");

        return Guid.Parse(claim.Value);
    }

    public static string GetEmail(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.Email);

        if (claim == null)
            throw new UnauthorizedAccessException("Email claim missing");

        return claim.Value;
    }

    public static string GetRole(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.Role);

        if (claim == null)
            throw new UnauthorizedAccessException("Role claim missing");

        return claim.Value;
    }
}
