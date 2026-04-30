using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace RotinaXP.API.Authorization;

public sealed class AdminHandler : AuthorizationHandler<AdminRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Check role claim
            var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);
            if (roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        // Try to inspect header for development scenarios
        var httpContext = context.Resource as HttpContext;
        if (httpContext != null)
        {
            if (httpContext.Request.Headers.TryGetValue("X-User-Role", out var headerVal))
            {
                if (string.Equals(headerVal.ToString(), "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }
        }

        return Task.CompletedTask;
    }
}
