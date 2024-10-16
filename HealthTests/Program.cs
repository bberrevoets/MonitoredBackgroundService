using Berrevoets;
using Berrevoets.Extensions;
using HealthTests;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection("WorkerOptions"));

builder.Services.AddMonitoredWorker<MyWorker>(options =>
{
    options.NumberOfTasks = 2;
});

//builder.Services.AddMonitoredWorker<MyWorker>(builder.Configuration);

builder.Services.AddHostedService<HealthMonitorWorker>();

var host = builder.Build();

host.Run();