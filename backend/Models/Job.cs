namespace CarbonPulseScheduler.Api.Models;

public enum JobStatus
{
    Pending,
    Scheduled,
    Running,
    Completed,
    Cancelled
}

public class Job
{
    public Guid JobId { get; set; } = Guid.NewGuid();
    public string Region { get; set; } = string.Empty;
    public DateTimeOffset EarliestStart { get; set; }
    public DateTimeOffset LatestEnd { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTimeOffset? ScheduledStart { get; set; }
    public DateTimeOffset? ScheduledEnd { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public string? Rationale { get; set; }
}
