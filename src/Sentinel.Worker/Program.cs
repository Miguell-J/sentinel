using Sentinel.Infrastructure;
using Sentinel.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSentinelInfrastructure(builder.Configuration);
builder.Services.AddHostedService<IncidentObserver>();
await builder.Build().RunAsync();
