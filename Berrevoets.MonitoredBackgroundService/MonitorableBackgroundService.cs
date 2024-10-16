using Berrevoets.Interfaces;
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

    public bool MonitorHealth()
    {
        var isHealthy = true;
        var currentTime = DateTime.UtcNow;

        foreach (var task in _tasks)
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                    isHealthy = false;
                    _logger.LogError("Task {TaskId} has faulted.", task.Id);
                    break;
                case TaskStatus.Canceled:
                    isHealthy = false;
                    _logger.LogWarning("Task {TaskId} was canceled.", task.Id);
                    break;
                default:
                {
                    if (_taskLastExecution.ContainsKey(task.Id) &&
                        (currentTime - _taskLastExecution[task.Id]).TotalSeconds > 120)
                    {
                        isHealthy = false;
                        _logger.LogWarning("Task {TaskId} has not reported activity for over 120 seconds.", task.Id);
                    }

                    break;
                }
            }

        _logger.LogInformation("Health check result: {isHealthy}", isHealthy);

        return isHealthy;
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