using Berrevoets;
using Microsoft.Extensions.Options;

namespace HealthTests;

public class MyWorker : MonitorableBackgroundService
{
    public MyWorker(ILogger<MyWorker> logger, IOptions<WorkerOptions> options)
        : base(logger, options)
    {
    }

    protected override async Task DoWork(int taskId)
    {
        while (!_stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker {taskId} running at: {time}", taskId, DateTimeOffset.Now);

            LogTaskInformation(taskId);
            UpdateTaskExecutionTime(taskId);

            await Task.Delay(1000, _stoppingToken);
        }
    }
}