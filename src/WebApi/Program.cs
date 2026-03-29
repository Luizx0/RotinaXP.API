using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using RotinaXP.API.Application.Interfaces.Services;
using RotinaXP.API.Data;
using RotinaXP.API.Authorization;
using RotinaXP.API.Features.Rewards.UseCases;
using RotinaXP.API.Features.Tasks.UseCases;
using RotinaXP.API.Middleware;
using RotinaXP.API.Services;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? ["http://localhost:3000", "http://localhost:5173"];

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

var externalDbPassword = builder.Configuration["Database:Password"]
    ?? Environment.GetEnvironmentVariable("ROTINAXP_DB_PASSWORD");

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "RotinaXP.API";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "RotinaXP.Client";
var jwtExpiryMinutes = builder.Configuration.GetValue<int?>("Jwt:ExpiryMinutes") ?? 120;
var jwtSecret = builder.Configuration["Jwt:Key"]
    ?? Environment.GetEnvironmentVariable("ROTINAXP_JWT_KEY")
    ?? throw new InvalidOperationException("JWT secret key was not configured.");
var otelServiceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "RotinaXP.API";
var otlpEndpoint = builder.Configuration["OpenTelemetry:Otlp:Endpoint"];
var rateLimitPermitLimit = builder.Configuration.GetValue<int?>("RateLimiting:PermitLimit") ?? 100;
var rateLimitWindowSeconds = builder.Configuration.GetValue<int?>("RateLimiting:WindowSeconds") ?? 60;
var rateLimitQueueLimit = builder.Configuration.GetValue<int?>("RateLimiting:QueueLimit") ?? 0;

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ ";
});

if (jwtSecret.Length < 32)
    throw new InvalidOperationException("JWT secret key must have at least 32 characters.");

if (!string.IsNullOrWhiteSpace(externalDbPassword))
{
    var csBuilder = new NpgsqlConnectionStringBuilder(connectionString)
    {
        Password = externalDbPassword
    };
    connectionString = csBuilder.ConnectionString;
}

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
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
    var xmlFile = "RotinaXP.API.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
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
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IRewardService, RewardService>();
builder.Services.AddScoped<DailyProgressService>();
builder.Services.AddScoped<GetTasksPageUseCase>();
builder.Services.AddScoped<GetTaskByIdUseCase>();
builder.Services.AddScoped<CreateTaskUseCase>();
builder.Services.AddScoped<UpdateTaskUseCase>();
builder.Services.AddScoped<DeleteTaskUseCase>();
builder.Services.AddScoped<GetRewardsPageUseCase>();
builder.Services.AddScoped<GetRewardByIdUseCase>();
builder.Services.AddScoped<CreateRewardUseCase>();
builder.Services.AddScoped<UpdateRewardUseCase>();
builder.Services.AddScoped<DeleteRewardUseCase>();
builder.Services.AddScoped<RedeemRewardUseCase>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddSingleton<IAuthorizationHandler, ResourceOwnerHandler>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ResourceOwner", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new ResourceOwnerRequirement());
    });
});
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["live"])
    .AddDbContextCheck<ApplicationDbContext>("database", tags: ["ready"]);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = (context, _) =>
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("RateLimiter");

        logger.LogWarning(
            "Rate limit exceeded for {Path} with correlation id {CorrelationId}",
            context.HttpContext.Request.Path,
            context.HttpContext.TraceIdentifier);

        return ValueTask.CompletedTask;
    };

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();
        var partitionKey = userId ?? remoteIp ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitPermitLimit,
                Window = TimeSpan.FromSeconds(rateLimitWindowSeconds),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = rateLimitQueueLimit,
                AutoReplenishment = true
            });
    });
});
builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(otelServiceName))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation();

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            tracing.AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(otlpEndpoint);
            });
        }
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation();

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            metrics.AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(otlpEndpoint);
            });
        }
    });
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed",
            Type = "https://httpstatuses.com/400"
        };
        return new BadRequestObjectResult(problemDetails);
    };
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
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

app.UseHttpsRedirection();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
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
app.Run();

public partial class Program;
