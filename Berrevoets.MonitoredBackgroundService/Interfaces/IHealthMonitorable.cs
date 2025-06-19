namespace Berrevoets.MonitoredBackgroundService.Interfaces;

/// <summary>
///     Defines a contract for monitoring the health status of a component or service.
/// </summary>
/// <remarks>
///     Implementations of this interface are expected to provide a mechanism to assess
///     their current health status, typically by returning a <see cref="HealthStatus" /> value.
///     This interface is commonly used in scenarios where the health of background services
///     or other long-running tasks needs to be monitored.
/// </remarks>
public interface IHealthMonitorable
{
    /// <summary>
    ///     Monitors the current health status of the component or service.
    /// </summary>
    /// <returns>
    ///     A <see cref="HealthStatus" /> value indicating the current health of the component or service.
    /// </returns>
    /// <remarks>
    ///     This method provides a mechanism to assess the operational state of the implementing component.
    ///     It is typically used to determine whether the component is functioning as expected, experiencing
    ///     minor issues, or is in a critical state.
    /// </remarks>
    HealthStatus MonitorHealth();
}
