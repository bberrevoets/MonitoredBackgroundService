using Berrevoets.Interfaces;
using Berrevoets.MonitoredBackgroundService.Interfaces;

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
                var healthStatus = monitorable.MonitorHealth();
                if (healthStatus == HealthStatus.Healthy)
                {
                    Console.WriteLine("All systems are healthy.");
                }
                else if (healthStatus == HealthStatus.Degraded)
                {
                    Console.WriteLine("Some tasks are taking longer than expected.");
                }
                else
                {
                    Console.WriteLine("Some tasks are unhealthy.");
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }

    #endregion
}