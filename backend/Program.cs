using CarbonPulseScheduler.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Register services as singletons (in-memory state)
builder.Services.AddSingleton<IVirtualClock, VirtualClock>();
builder.Services.AddSingleton<IJobRepository, InMemoryJobRepository>();
builder.Services.AddSingleton<ICarbonIntensityProvider, MockCarbonProvider>();
builder.Services.AddSingleton<IJobScheduler, CarbonAwareScheduler>();
builder.Services.AddHostedService<JobLifecycleService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseCors();
app.MapControllers();

app.Run();
