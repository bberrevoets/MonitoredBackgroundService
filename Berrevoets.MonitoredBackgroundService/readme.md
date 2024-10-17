# Berrevoets.MonitoredBackgroundService

A library for creating monitored background services with health checks and task monitoring in .NET applications.

## Overview

`Berrevoets.MonitoredBackgroundService` provides a base class and utilities to simplify the creation of background services that require health monitoring and task management. It allows you to:

- Create background services that run multiple tasks.
- Monitor the health of these tasks using customizable health checks.
- Configure task settings such as the number of tasks and maximum task duration.
- Receive detailed health statuses (`Healthy`, `Degraded`, `Unhealthy`) based on task performance.

## Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package Berrevoets.MonitoredBackgroundService
```

Or via the NuGet Package Manager Console in Visual Studio:

```powershell
Install-Package Berrevoets.MonitoredBackgroundService
```

## Usage

### 1. Setting Up Configuration

First, define your `appsettings.json` to include the `WorkerOptions` settings:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "WorkerOptions": {
    "NumberOfTasks": 5,
    "MaxTaskDuration": "00:02:00" // Format: HH:mm:ss
  }
}
```

- **`NumberOfTasks`**: The number of concurrent tasks the worker should run.
- **`MaxTaskDuration`**: The maximum duration a task is allowed to run before being considered unhealthy.

### 2. Creating a Custom Worker

Inherit from `MonitorableBackgroundService` to create your custom worker:

```csharp
using Berrevoets;
using Microsoft.Extensions.Options;

namespace YourNamespace;

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

            // Your custom task logic here
            await Task.Delay(1000, _stoppingToken);
        }
    }
}
```

- **`DoWork`**: Override this method to implement the task's logic.
- **`taskId`**: Unique identifier for each task.
- Use `LogTaskInformation(taskId)` and `UpdateTaskExecutionTime(taskId)` to log and update the task's execution time.

### 3. Implementing a Health Monitor Worker

Create a worker to monitor the health of your tasks:

```csharp
using Berrevoets.Interfaces;
using Microsoft.Extensions.Hosting;

namespace YourNamespace;

public class HealthMonitorWorker : BackgroundService
{
    private readonly ILogger<HealthMonitorWorker> _logger;
    private readonly IEnumerable<IHealthMonitorable> _healthMonitorables;

    public HealthMonitorWorker(
        ILogger<HealthMonitorWorker> logger,
        IEnumerable<IHealthMonitorable> healthMonitorables)
    {
        _logger = logger;
        _healthMonitorables = healthMonitorables;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var monitorable in _healthMonitorables)
            {
                var healthStatus = monitorable.MonitorHealth();
                switch (healthStatus)
                {
                    case HealthStatus.Healthy:
                        _logger.LogInformation("All systems are healthy.");
                        break;
                    case HealthStatus.Degraded:
                        _logger.LogWarning("Some tasks are taking longer than expected.");
                        break;
                    case HealthStatus.Unhealthy:
                        _logger.LogError("Some tasks are unhealthy.");
                        break;
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }
}
```

- **`IHealthMonitorable`**: Interface that provides the `MonitorHealth()` method.
- The monitor checks the health status and logs messages accordingly.

### 4. Configuring Dependency Injection

In your `Program.cs`, configure the services and workers:

```csharp
using Berrevoets;
using Berrevoets.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        // Configure WorkerOptions from appsettings.json
        services.Configure<WorkerOptions>(context.Configuration.GetSection("WorkerOptions"));

        // Add your custom worker using the extension method
        services.AddMonitoredWorker<MyWorker>(context.Configuration);

        // Add the health monitor worker
        services.AddHostedService<HealthMonitorWorker>();
    });

await builder.RunConsoleAsync();
```

- **`AddMonitoredWorker<TWorker>`**: Extension method to register your worker with health monitoring.
    - Can accept configuration from `appsettings.json` or inline options.
    - Example with inline options:

      ```csharp
      services.AddMonitoredWorker<MyWorker>(options =>
      {
          options.NumberOfTasks = 2;
          options.MaxTaskDuration = TimeSpan.FromMinutes(2);
      });
      ```
      
### 5. Running the Application

Run your application, and you should see logs indicating the status of your tasks and any health issues detected by the monitor.

## Features

- **Task Management**: Easily manage multiple background tasks within a worker.
- **Health Monitoring**: Monitor tasks for health status, including `Healthy`, `Degraded`, and `Unhealthy`.
- **Configuration**: Customize the number of tasks and maximum task duration via configuration files or code.
- **Extensibility**: Create custom workers by inheriting from `MonitorableBackgroundService` and implementing your task logic.

## API Reference

### MonitorableBackgroundService

An abstract class that provides the infrastructure for monitored background services.

#### Methods to Override

- **`Task DoWork(int taskId)`**: Implement your task logic here.

#### Protected Methods

- **`void UpdateTaskExecutionTime(int taskId)`**: Updates the last execution time for a task.
- **`void LogTaskInformation(int taskId)`**: Logs information about the task.

### WorkerOptions

Configuration options for your worker.

- **`int NumberOfTasks`**: Number of concurrent tasks to run.
- **`TimeSpan MaxTaskDuration`**: Maximum allowed duration for a task before it's considered unhealthy.

### HealthStatus Enum

Represents the health status of the worker tasks.

- **`Healthy`**: All tasks are running within expected parameters.
- **`Degraded`**: Tasks are taking longer than expected (exceeding half of `MaxTaskDuration`).
- **`Unhealthy`**: Tasks have faulted, canceled, or exceeded `MaxTaskDuration`.

### IHealthMonitorable Interface

An interface that provides a method to monitor health.

- **`HealthStatus MonitorHealth()`**: Returns the current health status.

## Contributing

Contributions are welcome! Please submit a pull request or open an issue to discuss improvements or report bugs.

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/bberrevoets/MonitoredBackgroundService/blob/main/LICENSE) file for details.

## Acknowledgments

- Inspired by the need for robust background task management and health monitoring in .NET applications.

## Contact

For questions or support, please contact [Bert Berrevoets](mailto:your-email@example.com).

---

Happy coding!