using Berrevoets.MonitoredBackgroundService;
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
        while (!StoppingToken.IsCancellationRequested)
        {
            Logger.LogInformation("Worker {taskId} running at: {time}", taskId, DateTimeOffset.Now);

            LogTaskInformation(taskId);
            UpdateTaskExecutionTime(taskId);

            await Task.Delay(1000, StoppingToken);
        }
    }
}
