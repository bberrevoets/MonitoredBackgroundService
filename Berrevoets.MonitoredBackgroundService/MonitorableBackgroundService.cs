using Berrevoets.MonitoredBackgroundService.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Berrevoets.MonitoredBackgroundService;

/// <summary>
///     Represents an abstract base class for a background service that supports health monitoring
///     and task execution tracking.
/// </summary>
/// <remarks>
///     This class extends <see cref="Microsoft.Extensions.Hosting.BackgroundService" /> and implements
///     <see cref="Berrevoets.MonitoredBackgroundService.Interfaces.IHealthMonitorable" /> to provide
///     functionality for monitoring the health of background tasks. It manages task execution times,
///     logs task statuses, and ensures tasks adhere to configured constraints.
/// </remarks>
public abstract class MonitorableBackgroundService : BackgroundService, IHealthMonitorable
{
    private readonly Dictionary<int, DateTime> _taskLastExecution = new();
    private readonly List<Task> _tasks = [];
    protected readonly ILogger Logger;
    private int _taskCounter;
    protected CancellationToken StoppingToken;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MonitorableBackgroundService" /> class.
    /// </summary>
    /// <param name="logger">The logger instance used to log information, warnings, and errors.</param>
    /// <param name="options">The configuration options for the worker, including task constraints.</param>
    /// <remarks>
    ///     This constructor sets up the background service with the provided logger and worker options.
    ///     The options define the behavior and constraints for the tasks managed by the service.
    /// </remarks>
    protected MonitorableBackgroundService(ILogger logger, IOptions<WorkerOptions> options)
    {
        Logger = logger;
        Options = options.Value;
    }

    private WorkerOptions Options { get; }

    #region Implementation of IHealthMonitorable

    /// <summary>
    ///     Monitors the health of the background service by evaluating the status and execution times
    ///     of its tasks.
    /// </summary>
    /// <returns>
    ///     A <see cref="HealthStatus" /> value indicating the overall health of the background service.
    ///     Possible values are:
    ///     <list type="bullet">
    ///         <item>
    ///             <description><see cref="HealthStatus.Healthy" />: All tasks are running within expected parameters.</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="HealthStatus.Degraded" />: One or more tasks are taking longer than expected but
    ///                 are still running.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="HealthStatus.Unhealthy" />: One or more tasks have faulted, been canceled, or
    ///                 exceeded the maximum allowed duration.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     This method evaluates the health of the background service by iterating through its tasks.
    ///     It checks for faulted or canceled tasks and compares task execution durations against configured
    ///     thresholds. Logs are generated to provide detailed information about the health status of tasks.
    /// </remarks>
    public HealthStatus MonitorHealth()
    {
        var currentTime = DateTime.UtcNow;
        var healthStatus = HealthStatus.Healthy;

        foreach (var task in _tasks)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                case TaskStatus.Canceled:
                    healthStatus = HealthStatus.Unhealthy;
                    Logger.LogError("Task {TaskId} has faulted or was canceled.", task.Id);
                    break;

                default:
                    if (_taskLastExecution.TryGetValue(task.Id, out var value))
                    {
                        var taskDuration = currentTime - value;

                        if (taskDuration > Options.MaxTaskDuration)
                        {
                            healthStatus = HealthStatus.Unhealthy;
                            Logger.LogWarning(
                                "Task {TaskId} has exceeded the maximum allowed duration of {MaxTaskDuration}.",
                                task.Id, Options.MaxTaskDuration);
                        }
                        else if (taskDuration > Options.MaxTaskDuration / 2)
                        {
                            healthStatus = HealthStatus.Degraded;
                            Logger.LogWarning("Task {TaskId} is taking longer than expected (Degraded state).",
                                task.Id);
                        }
                    }

                    break;
            }

            // If a task is unhealthy, we can break early to avoid further checks
            if (healthStatus == HealthStatus.Unhealthy) break;
        }

        Logger.LogInformation("Health check result: {HealthStatus}", healthStatus);

        return healthStatus;
    }

    #endregion

    #region Overrides of BackgroundService

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        StoppingToken = stoppingToken;

        for (var i = 0; i < Options.NumberOfTasks; i++)
        {
            var worker = StartTaskWithId(DoWork, ++_taskCounter);
            _tasks.Add(worker);
        }

        await Task.WhenAll(_tasks);
    }

    #endregion

    private Task StartTaskWithId(Func<int, Task> taskFunc, int taskId)
    {
        var task = Task.Run(() => taskFunc(taskId), StoppingToken);
        _taskLastExecution[taskId] = DateTime.UtcNow; // Initialize the timestamp
        return task;
    }

    /// <summary>
    ///     Executes the core logic of the background service for a specific task.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task being executed.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <remarks>
    ///     This method is abstract and must be implemented by derived classes to define the specific
    ///     work to be performed by each task. The method is invoked for each task managed by the service,
    ///     and the <paramref name="taskId" /> parameter provides a way to differentiate between tasks.
    /// </remarks>
    protected abstract Task DoWork(int taskId);

    protected void UpdateTaskExecutionTime(int taskId)
    {
        if (_taskLastExecution.ContainsKey(taskId)) _taskLastExecution[taskId] = DateTime.UtcNow;
    }

    protected void LogTaskInformation(int taskId)
    {
        Logger.LogInformation("Worker {taskId} running at: {time}", taskId, DateTimeOffset.Now);
    }
}
