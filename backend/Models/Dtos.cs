namespace CarbonPulseScheduler.Api.Models;

public class CreateJobRequest
{
    public string Region { get; set; } = string.Empty;
    public DateTimeOffset EarliestStart { get; set; }
    public DateTimeOffset LatestEnd { get; set; }
    public int DurationMinutes { get; set; }
}

public class JobResponse
{
    public Guid JobId { get; set; }
    public string Region { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset EarliestStart { get; set; }
    public DateTimeOffset LatestEnd { get; set; }
    public int DurationMinutes { get; set; }
    public DateTimeOffset? ScheduledStart { get; set; }
    public DateTimeOffset? ScheduledEnd { get; set; }
    public string? Rationale { get; set; }

    public static JobResponse FromJob(Job job) => new()
    {
        JobId = job.JobId,
        Region = job.Region,
        Status = job.Status.ToString(),
        EarliestStart = job.EarliestStart,
        LatestEnd = job.LatestEnd,
        DurationMinutes = (int)job.Duration.TotalMinutes,
        ScheduledStart = job.ScheduledStart,
        ScheduledEnd = job.ScheduledEnd,
        Rationale = job.Rationale
    };
}

public class ClockRequest
{
    public string Mode { get; set; } = string.Empty;   // set | reset | accelerate | pause
    public string? Value { get; set; }
}

public class ClockResponse
{
    public DateTimeOffset VirtualNow { get; set; }
    public double SpeedMultiplier { get; set; }
    public bool Paused { get; set; }
}
