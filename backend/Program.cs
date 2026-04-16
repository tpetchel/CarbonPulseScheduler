using CarbonPulseScheduler.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Register services as singletons (in-memory state)
builder.Services.AddSingleton<IVirtualClock, VirtualClock>();
builder.Services.AddSingleton<IJobRepository, InMemoryJobRepository>();
builder.Services.AddHostedService<JobLifecycleService>();

// Carbon intensity provider: "Mock" (default) or "Sdk"
var carbonProvider = builder.Configuration.GetValue<string>("CarbonProvider") ?? "Mock";
if (carbonProvider.Equals("Sdk", StringComparison.OrdinalIgnoreCase))
{
    var sdkBaseUrl = builder.Configuration.GetValue<string>("CarbonAwareSdk:BaseUrl") ?? "http://localhost:8080";
    builder.Services.AddHttpClient<ICarbonIntensityProvider, CarbonAwareSdkProvider>(client =>
    {
        client.BaseAddress = new Uri(sdkBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(15);
    });
}
else
{
    builder.Services.AddSingleton<ICarbonIntensityProvider, MockCarbonProvider>();
}

// Scheduler: "CarbonAware" (default) or "Dummy"
var scheduler = builder.Configuration.GetValue<string>("Scheduler") ?? "CarbonAware";
if (scheduler.Equals("Dummy", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddSingleton<IJobScheduler, DummyScheduler>();
else
    builder.Services.AddSingleton<IJobScheduler, CarbonAwareScheduler>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseCors();
app.MapControllers();

app.Run();
