using System.Security.Claims;

namespace RotinaXP.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetAuthenticatedUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(value, out var userId) ? userId : null;
    }
}
