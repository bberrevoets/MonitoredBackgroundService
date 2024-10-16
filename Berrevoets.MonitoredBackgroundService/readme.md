# Berrevoets.MonitoredBackgroundService

A library for creating monitored background services with health checks and task monitoring.

## Installation

Install via NuGet:

```bash
dotnet add package Berrevoets.MonitoredBackgroundService
```

## Usage

To create a monitored background service, inherit from the MonitorableBackgroundService class and override the DoWork method:

```C#
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
            LogTaskInformation(taskId);
            UpdateTaskExecutionTime(taskId);

            // Your task logic here
            await Task.Delay(1000, _stoppingToken);
        }
    }
}
```
