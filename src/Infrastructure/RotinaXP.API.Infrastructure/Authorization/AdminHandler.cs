using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace RotinaXP.API.Authorization;

public sealed class AdminHandler : AuthorizationHandler<AdminRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Check role claim
            var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);
            if (roles.Contains("Admin", StringComparer.OrdinalIgnoreCase)
                || roles.Contains("SUPERUSER", StringComparer.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}
