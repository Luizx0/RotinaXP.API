using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using RotinaXP.API.Application.Interfaces.Services;
using RotinaXP.API.Authorization;
using RotinaXP.API.Common;
using RotinaXP.API.Data;
using RotinaXP.API.Features.DailyProgress.UseCases;
using RotinaXP.API.Features.Rewards.UseCases;
using RotinaXP.API.Features.Tasks.UseCases;
using RotinaXP.API.Features.Users.UseCases;
using RotinaXP.API.Options;
using RotinaXP.API.Security;
using RotinaXP.API.Services;

namespace RotinaXP.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Security
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

        // Core services (via interfaces)
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IRewardService, RewardService>();
        services.AddScoped<IDailyProgressService, DailyProgressService>();
        services.AddScoped<JwtTokenService>();

        // Use cases - Tasks
        services.AddScoped<GetTasksPageUseCase>();
        services.AddScoped<GetTaskByIdUseCase>();
        services.AddScoped<CreateTaskUseCase>();
        services.AddScoped<UpdateTaskUseCase>();
        services.AddScoped<DeleteTaskUseCase>();

        // Use cases - Rewards
        services.AddScoped<GetRewardsPageUseCase>();
        services.AddScoped<GetRewardByIdUseCase>();
        services.AddScoped<CreateRewardUseCase>();
        services.AddScoped<UpdateRewardUseCase>();
        services.AddScoped<DeleteRewardUseCase>();
        services.AddScoped<RedeemRewardUseCase>();

        // Use cases - Users
        services.AddScoped<GetUsersPageUseCase>();
        services.AddScoped<GetUserByIdUseCase>();
        services.AddScoped<UpdateUserUseCase>();
        services.AddScoped<DeleteUserUseCase>();

        // Use cases - DailyProgress
        services.AddScoped<GetDailyProgressPageUseCase>();
        services.AddScoped<GetDailyProgressByIdUseCase>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, JwtOptions options)
    {
        if (options.Key.Length < 32)
            throw new InvalidOperationException("JWT secret key must have at least 32 characters.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = options.Issuer,
                    ValidAudience = options.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }

    public static IServiceCollection AddResourceOwnerAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, ResourceOwnerHandler>();
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AppConstants.Policies.ResourceOwner, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new ResourceOwnerRequirement());
            });
        });

        return services;
    }

    public static IServiceCollection AddSwaggerWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "RotinaXP API",
                Version = "v1",
                Description = "REST API for managing users, tasks, progress, and rewards",
                Contact = new OpenApiContact
                {
                    Name = "RotinaXP",
                    Email = "contato@rotinaxp.local"
                }
            });

            var xmlPath = Path.Combine(AppContext.BaseDirectory, "RotinaXP.API.xml");
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Provide a valid JWT token"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, AppCorsOptions options)
    {
        services.AddCors(cors =>
        {
            cors.AddPolicy(AppConstants.Cors.PolicyName, policy =>
            {
                policy.WithOrigins(options.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    public static IServiceCollection AddGlobalRateLimiting(this IServiceCollection services, RateLimitingOptions options)
    {
        services.AddRateLimiter(rl =>
        {
            rl.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            rl.OnRejected = (context, _) =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("RateLimiter");

                logger.LogWarning(
                    "Rate limit exceeded for {Path} with correlation id {CorrelationId}",
                    context.HttpContext.Request.Path,
                    context.HttpContext.TraceIdentifier);

                return ValueTask.CompletedTask;
            };

            rl.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
            {
                var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var remoteIp = ctx.Connection.RemoteIpAddress?.ToString();
                var key = userId ?? remoteIp ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = options.PermitLimit,
                    Window = TimeSpan.FromSeconds(options.WindowSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = options.QueueLimit,
                    AutoReplenishment = true
                });
            });
        });

        return services;
    }

    public static IServiceCollection AddOtel(this IServiceCollection services, OtelOptions options)
    {
        var otlpEndpoint = options.Otlp.Endpoint;

        services
            .AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(options.ServiceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    tracing.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    metrics.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
            });

        return services;
    }

    public static IServiceCollection AddHealthEndpoints(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
            .AddDbContextCheck<ApplicationDbContext>("database", tags: ["ready"]);

        return services;
    }
}
