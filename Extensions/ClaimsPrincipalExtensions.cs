using System.Security.Claims;

namespace RotinaXP.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetAuthenticatedUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }
}
