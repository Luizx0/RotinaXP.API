using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RotinaXP.API.Data;
using RotinaXP.API.Extensions;
using RotinaXP.API.Options;
using RotinaXP.API.Models;
using RotinaXP.API.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ ";
});

builder.Services.AddOpenApi();

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwtOptions.Key))
    jwtOptions.Key = Environment.GetEnvironmentVariable("ROTINAXP_JWT_KEY") ?? string.Empty;

var databaseOptions = builder.Configuration
    .GetSection(DatabaseOptions.SectionName)
    .Get<DatabaseOptions>()
    ?? new DatabaseOptions();

if (string.IsNullOrWhiteSpace(databaseOptions.Password))
    databaseOptions.Password = Environment.GetEnvironmentVariable("ROTINAXP_DB_PASSWORD") ?? string.Empty;

var corsOptions = builder.Configuration
    .GetSection(AppCorsOptions.SectionName)
    .Get<AppCorsOptions>()
    ?? new AppCorsOptions();

var rateLimitingOptions = builder.Configuration
    .GetSection(RateLimitingOptions.SectionName)
    .Get<RateLimitingOptions>()
    ?? new RateLimitingOptions();

var superUserOptions = builder.Configuration
    .GetSection(SuperUserOptions.SectionName)
    .Get<SuperUserOptions>()
    ?? new SuperUserOptions();

superUserOptions.Email = Environment.GetEnvironmentVariable("ROTINAXP_SUPERUSER_EMAIL")
    ?? superUserOptions.Email;
superUserOptions.Password = Environment.GetEnvironmentVariable("ROTINAXP_SUPERUSER_PASSWORD")
    ?? superUserOptions.Password;

var otelOptions = builder.Configuration
    .GetSection(OtelOptions.SectionName)
    .Get<OtelOptions>()
    ?? new OtelOptions();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

if (!string.IsNullOrWhiteSpace(databaseOptions.Password))
{
    var connectionBuilder = new NpgsqlConnectionStringBuilder(connectionString)
    {
        Password = databaseOptions.Password
    };

    connectionString = connectionBuilder.ConnectionString;
}

builder.Services
    .AddDatabase(connectionString)
    .AddApplicationServices()
    .AddJwtAuthentication(jwtOptions)
    .AddResourceOwnerAuthorization()
    .AddSwaggerWithAuth()
    .AddCorsPolicy(corsOptions)
    .AddGlobalRateLimiting(rateLimitingOptions)
    .AddOtel(otelOptions)
    .AddHealthEndpoints();

builder.Services.AddProblemDetails();
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

var app = builder.Build();

await SeedSuperUserAsync(app.Services, superUserOptions);

app.UseRotinaXpDevelopmentTools();
app.UseRotinaXpPipeline();
app.MapControllers();
app.MapHealthEndpoints();

static async Task SeedSuperUserAsync(IServiceProvider services, SuperUserOptions options)
{
    if (string.IsNullOrWhiteSpace(options.Email) || string.IsNullOrWhiteSpace(options.Password))
        return;

    if (options.Password.Length < 8)
        throw new InvalidOperationException("ROTINAXP_SUPERUSER_PASSWORD must have at least 8 characters.");

    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    var email = options.Email.Trim().ToLowerInvariant();
    var user = await context.Users.FirstOrDefaultAsync(candidate => candidate.Email == email);

    if (user == null)
    {
        context.Users.Add(new User
        {
            Name = "Superuser",
            Email = email,
            PasswordHash = passwordHasher.Hash(options.Password)
        });
    }
    else if (!passwordHasher.Verify(options.Password, user.PasswordHash))
    {
        user.PasswordHash = passwordHasher.Hash(options.Password);
    }

    await context.SaveChangesAsync();
}

app.Run();

public partial class Program;
