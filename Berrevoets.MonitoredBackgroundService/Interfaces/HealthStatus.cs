namespace Berrevoets.MonitoredBackgroundService.Interfaces;

/// <summary>
///     Represents the health status of a monitored background service or task.
/// </summary>
/// <summary>
///     Indicates that the service or task is operating normally and within expected parameters.
/// </summary>
/// <summary>
///     Indicates that the service or task is experiencing issues but is still operational.
/// </summary>
/// <summary>
///     Indicates that the service or task is not operational due to faults, cancellations, or other critical issues.
/// </summary>
public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy
}
