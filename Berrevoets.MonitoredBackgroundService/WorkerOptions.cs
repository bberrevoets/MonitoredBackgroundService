namespace Berrevoets.MonitoredBackgroundService;

/// <summary>
///     Represents configuration options for a worker in the monitored background service.
/// </summary>
/// <remarks>
///     This class provides settings to control the behavior of a worker, such as the number of tasks
///     it can handle and the maximum duration allowed for each task.
/// </remarks>
public class WorkerOptions
{
    /// <summary>
    ///     Gets or sets the number of tasks that the worker can execute concurrently.
    /// </summary>
    /// <value>
    ///     The default value is <c>1</c>.
    /// </value>
    /// <remarks>
    ///     This property determines the level of concurrency for the worker. Increasing the value
    ///     allows the worker to handle more tasks simultaneously, but it may also increase resource usage.
    /// </remarks>
    public int NumberOfTasks { get; set; } = 1;

    /// <summary>
    ///     Gets or sets the maximum duration allowed for a task to execute.
    /// </summary>
    /// <value>
    ///     A <see cref="TimeSpan" /> representing the maximum time a task is allowed to run before it is considered unhealthy.
    ///     The default value is 120 seconds.
    /// </value>
    /// <remarks>
    ///     This property is used to monitor the health of tasks in the background service. If a task exceeds this duration,
    ///     it may trigger warnings or health status changes.
    /// </remarks>
    public TimeSpan MaxTaskDuration { get; init; } = TimeSpan.FromSeconds(120);
}
