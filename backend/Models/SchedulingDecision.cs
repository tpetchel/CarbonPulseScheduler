namespace CarbonPulseScheduler.Api.Models;

public class SchedulingDecision
{
    public DateTimeOffset RecommendedStart { get; set; }
    public string Rationale { get; set; } = string.Empty;
}
