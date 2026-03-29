using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using RotinaXP.API.Common;
using RotinaXP.API.Middleware;

namespace RotinaXP.API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseRotinaXpDevelopmentTools(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "RotinaXP API v1");
                options.RoutePrefix = string.Empty;
            });
        }

        return app;
    }

    public static WebApplication UseRotinaXpPipeline(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseCors(AppConstants.Cors.PolicyName);
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    public static WebApplication MapHealthEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live")
        });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        return app;
    }
}
