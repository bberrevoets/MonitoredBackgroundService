using Berrevoets.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Berrevoets.Extensions;

public static class ServiceCollectionExtensions
{
    // Overload 1: Accepts an action to configure WorkerOptions
    public static IServiceCollection AddMonitoredWorker<TWorker>(
        this IServiceCollection services,
        Action<WorkerOptions> configureOptions) where TWorker : class, IHealthMonitorable, IHostedService
    {
        services.Configure(configureOptions);
        RegisterWorker<TWorker>(services);
        return services;
    }

    // Overload 2: Accepts IConfiguration to bind WorkerOptions from a configuration section
    public static IServiceCollection AddMonitoredWorker<TWorker>(
        this IServiceCollection services,
        IConfiguration configuration) where TWorker : class, IHealthMonitorable, IHostedService
    {
        services.Configure<WorkerOptions>(configuration.GetSection("WorkerOptions"));
        RegisterWorker<TWorker>(services);
        return services;
    }

    // Helper method to register the worker
    private static void RegisterWorker<TWorker>(IServiceCollection services)
        where TWorker : class, IHealthMonitorable, IHostedService
    {
        services.AddSingleton<TWorker>();
        services.AddSingleton<IHealthMonitorable>(sp => sp.GetRequiredService<TWorker>());
        services.AddHostedService(sp => sp.GetRequiredService<TWorker>());
    }
}