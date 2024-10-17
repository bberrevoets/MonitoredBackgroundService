using Berrevoets;
using Berrevoets.Extensions;
using HealthTests;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", true, true);
builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection("WorkerOptions"));

builder.Services.AddMonitoredWorker<MyWorker>(options => { options.NumberOfTasks = 2; });

// Or use this instead of the previous way of adding the worker
// builder.Services.AddMonitoredWorker<MyWorker>(builder.Configuration);

builder.Services.AddHostedService<HealthMonitorWorker>();

var host = builder.Build();

host.Run();