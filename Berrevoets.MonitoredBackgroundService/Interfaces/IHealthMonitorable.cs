using Berrevoets.MonitoredBackgroundService.Interfaces;

namespace Berrevoets.Interfaces;

public interface IHealthMonitorable
{
    HealthStatus MonitorHealth();
}