namespace RotinaXP.API.Workers;

public interface IScheduledWorker
{
    string Name { get; }
    TimeSpan Interval { get; }
    Task ExecuteAsync(CancellationToken cancellationToken);
}
