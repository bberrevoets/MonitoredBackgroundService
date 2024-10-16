using Berrevoets.Interfaces;

namespace HealthTests;

public class HealthMonitorWorker : BackgroundService
{
    private readonly ILogger<HealthMonitorWorker> _logger;
    private readonly IEnumerable<IHealthMonitorable> _healthMonitorables;

    public HealthMonitorWorker(ILogger<HealthMonitorWorker> logger, IEnumerable<IHealthMonitorable> healthMonitorables)
    {
        _logger = logger;
        _healthMonitorables = healthMonitorables;
    }
    
    #region Overrides of BackgroundService

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            foreach (var monitorable in _healthMonitorables)
            {
                if (monitorable.MonitorHealth())
                {
                    _logger.LogInformation("All monitored services are healthy.");
                }
                else
                {
                    _logger.LogWarning("A monitored service is not healthy!");
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }

    #endregion
}