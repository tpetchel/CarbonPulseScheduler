using CarbonPulseScheduler.Api.Models;

namespace CarbonPulseScheduler.Api.Services;

public class JobLifecycleService : BackgroundService
{
    private readonly IJobRepository _repo;
    private readonly IVirtualClock _clock;

    public JobLifecycleService(IJobRepository repo, IVirtualClock clock)
    {
        _repo = repo;
        _clock = clock;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = _clock.Now;

            foreach (var job in _repo.List())
            {
                if (job.Status == JobStatus.Scheduled && job.ScheduledStart.HasValue && now >= job.ScheduledStart.Value)
                {
                    job.Status = JobStatus.Running;
                    _repo.Update(job);
                }
                else if (job.Status == JobStatus.Running && job.ScheduledEnd.HasValue && now >= job.ScheduledEnd.Value)
                {
                    job.Status = JobStatus.Completed;
                    _repo.Update(job);
                }
            }

            await Task.Delay(500, stoppingToken);
        }
    }
}
