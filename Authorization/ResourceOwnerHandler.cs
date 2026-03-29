using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace RotinaXP.API.Authorization;

public sealed class ResourceOwnerHandler : AuthorizationHandler<ResourceOwnerRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOwnerRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return Task.CompletedTask;

        if (!TryGetUserIdClaim(context.User, out var claimUserId))
            return Task.CompletedTask;

        var httpContext = context.Resource as HttpContext;
        if (httpContext == null)
            return Task.CompletedTask;

        if (!TryGetRouteUserId(httpContext, out var routeUserId))
            return Task.CompletedTask;

        if (claimUserId == routeUserId)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }

    private static bool TryGetUserIdClaim(ClaimsPrincipal user, out int userId)
    {
        userId = 0;
        var claimValue = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claimValue, out userId);
    }

    private static bool TryGetRouteUserId(HttpContext context, out int userId)
    {
        userId = 0;

        if (TryParseRouteInt(context, "userId", out userId))
            return true;

        if (TryParseRouteInt(context, "id", out userId))
            return true;

        return false;
    }

    private static bool TryParseRouteInt(HttpContext context, string key, out int value)
    {
        value = 0;
        if (!context.Request.RouteValues.TryGetValue(key, out var rawValue) || rawValue == null)
            return false;

        return int.TryParse(rawValue.ToString(), out value);
    }
}
