using Berrevoets.Interfaces;
using Berrevoets.MonitoredBackgroundService.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Berrevoets;

public abstract class MonitorableBackgroundService : BackgroundService, IHealthMonitorable
{
    protected readonly ILogger _logger;
    private readonly Dictionary<int, DateTime> _taskLastExecution = new();
    private readonly List<Task> _tasks = [];
    protected CancellationToken _stoppingToken;
    private int _taskCounter;

    protected MonitorableBackgroundService(ILogger logger, IOptions<WorkerOptions> options)
    {
        _logger = logger;
        Options = options.Value;
    }

    protected WorkerOptions Options { get; }

    #region Implementation of IHealthMonitorable

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
                    _logger.LogError("Task {TaskId} has faulted or was canceled.", task.Id);
                    break;

                default:
                    if (_taskLastExecution.ContainsKey(task.Id))
                    {
                        var taskDuration = currentTime - _taskLastExecution[task.Id];

                        if (taskDuration > Options.MaxTaskDuration)
                        {
                            healthStatus = HealthStatus.Unhealthy;
                            _logger.LogWarning(
                                "Task {TaskId} has exceeded the maximum allowed duration of {MaxTaskDuration}.",
                                task.Id, Options.MaxTaskDuration);
                        }
                        else if (taskDuration > Options.MaxTaskDuration / 2)
                        {
                            // Task is taking longer than half the MaxTaskDuration
                            if (healthStatus != HealthStatus.Unhealthy) healthStatus = HealthStatus.Degraded;
                            _logger.LogWarning("Task {TaskId} is taking longer than expected (Degraded state).",
                                task.Id);
                        }
                    }

                    break;
            }

            // If a task is unhealthy, we can break early to avoid further checks
            if (healthStatus == HealthStatus.Unhealthy) break;
        }

        _logger.LogInformation("Health check result: {HealthStatus}", healthStatus);

        return healthStatus;
    }

    #endregion

    #region Overrides of BackgroundService

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stoppingToken = stoppingToken;

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
        var task = Task.Run(() => taskFunc(taskId), _stoppingToken);
        _taskLastExecution[taskId] = DateTime.UtcNow; // Initialize the timestamp
        return task;
    }

    protected abstract Task DoWork(int taskId);

    protected void UpdateTaskExecutionTime(int taskId)
    {
        if (_taskLastExecution.ContainsKey(taskId)) _taskLastExecution[taskId] = DateTime.UtcNow;
    }

    protected void LogTaskInformation(int taskId)
    {
        _logger.LogInformation("Worker {taskId} running at: {time}", taskId, DateTimeOffset.Now);
    }
}