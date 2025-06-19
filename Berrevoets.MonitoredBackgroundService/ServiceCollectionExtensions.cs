using Berrevoets.MonitoredBackgroundService.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Berrevoets.MonitoredBackgroundService;

/// <summary>
///     Provides extension methods for registering monitored background workers in the dependency injection container.
/// </summary>
/// <remarks>
///     This static class includes methods to add and configure workers that implement <see cref="IHealthMonitorable" />
///     and <see cref="IHostedService" />. These workers can be monitored for health status and are registered as
///     singleton services in the dependency injection container.
/// </remarks>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers a monitored background worker of type <typeparamref name="TWorker" /> in the dependency injection
    ///     container.
    /// </summary>
    /// <typeparam name="TWorker">
    ///     The type of the worker to register. Must implement <see cref="IHealthMonitorable" /> and
    ///     <see cref="IHostedService" />.
    /// </typeparam>
    /// <param name="services">
    ///     The <see cref="IServiceCollection" /> to which the worker will be added.
    /// </param>
    /// <param name="configureOptions">
    ///     An <see cref="Action{T}" /> to configure the <see cref="WorkerOptions" /> for the worker.
    /// </param>
    /// <returns>
    ///     The updated <see cref="IServiceCollection" /> to allow for method chaining.
    /// </returns>
    /// <remarks>
    ///     This method configures the worker's options using the provided <paramref name="configureOptions" /> delegate
    ///     and registers the worker as a singleton service. The worker is also registered as both an
    ///     <see cref="IHealthMonitorable" /> and an <see cref="IHostedService" /> to enable health monitoring and background
    ///     execution.
    /// </remarks>
    /// <example>
    ///     <code>
    /// builder.Services.AddMonitoredWorker<MyWorker>
    ///             (options =>
    ///             {
    ///             options.NumberOfTasks = 5;
    ///             options.MaxTaskDuration = TimeSpan.FromMinutes(5);
    ///             });
    /// </code>
    /// </example>
    public static IServiceCollection AddMonitoredWorker<TWorker>(
        this IServiceCollection services,
        Action<WorkerOptions> configureOptions) where TWorker : class, IHealthMonitorable, IHostedService
    {
        services.Configure(configureOptions);
        RegisterWorker<TWorker>(services);
        return services;
    }

    /// <summary>
    ///     Registers a monitored background worker of type <typeparamref name="TWorker" /> in the dependency injection
    ///     container.
    /// </summary>
    /// <typeparam name="TWorker">
    ///     The type of the worker to register. Must implement <see cref="IHealthMonitorable" /> and
    ///     <see cref="IHostedService" />.
    /// </typeparam>
    /// <param name="services">
    ///     The <see cref="IServiceCollection" /> to which the worker will be added.
    /// </param>
    /// <param name="configuration">
    ///     The <see cref="IConfiguration" /> instance used to configure the worker options.
    /// </param>
    /// <returns>
    ///     The updated <see cref="IServiceCollection" /> instance.
    /// </returns>
    /// <remarks>
    ///     This method configures the worker using the "WorkerOptions" section of the provided <see cref="IConfiguration" />
    ///     and registers it as a singleton service. The worker is also registered as an <see cref="IHealthMonitorable" />
    ///     and an <see cref="IHostedService" />.
    /// </remarks>
    /// <example>
    ///     <code>
    /// var services = new ServiceCollection();
    /// var configuration = new ConfigurationBuilder()
    ///     .AddJsonFile("appsettings.json")
    ///     .Build();
    /// services.AddMonitoredWorker<MyWorker>(configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddMonitoredWorker<TWorker>(
        this IServiceCollection services,
        IConfiguration configuration) where TWorker : class, IHealthMonitorable, IHostedService
    {
        services.Configure<WorkerOptions>(configuration.GetSection("WorkerOptions"));
        RegisterWorker<TWorker>(services);
        return services;
    }

    /// <summary>
    ///     Registers a worker of type <typeparamref name="TWorker" /> in the dependency injection container.
    /// </summary>
    /// <typeparam name="TWorker">
    ///     The type of the worker to register. Must implement <see cref="IHealthMonitorable" /> and
    ///     <see cref="IHostedService" />.
    /// </typeparam>
    /// <param name="services">
    ///     The <see cref="IServiceCollection" /> to which the worker will be added.
    /// </param>
    /// <remarks>
    ///     This method registers the worker as a singleton service in the dependency injection container.
    ///     The worker is also registered as both an <see cref="IHealthMonitorable" /> and an <see cref="IHostedService" />
    ///     to enable health monitoring and background execution.
    /// </remarks>
    private static void RegisterWorker<TWorker>(IServiceCollection services)
        where TWorker : class, IHealthMonitorable, IHostedService
    {
        services.AddSingleton<TWorker>();
        services.AddSingleton<IHealthMonitorable>(sp => sp.GetRequiredService<TWorker>());
        services.AddHostedService(sp => sp.GetRequiredService<TWorker>());
    }
}
