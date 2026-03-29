using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RotinaXP.API.Workers;

/// <summary>
/// Worker para redefinir streaks de usuarios que nao completaram tarefas no dia anterior.
/// Registrar no DI via: services.AddHostedService&lt;StreakResetWorker&gt;()
/// </summary>
public sealed class StreakResetWorker : BackgroundService, IScheduledWorker
{
    public string Name => "StreakResetWorker";
    public TimeSpan Interval => TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StreakResetWorker> _logger;

    public StreakResetWorker(IServiceScopeFactory scopeFactory, ILogger<StreakResetWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExecuteAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "{Worker} failed during execution.", Name);
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    async Task IScheduledWorker.ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Worker} running at {Time}.", Name, DateTimeOffset.UtcNow);

        await using var scope = _scopeFactory.CreateAsyncScope();

        // TODO: injetar repositório de progresso diário e redefinir streaks
        // var progressRepo = scope.ServiceProvider.GetRequiredService<IDailyProgressRepository>();
        // await progressRepo.ResetInactiveStreaksAsync(cancellationToken);

        await Task.CompletedTask;
    }
}
