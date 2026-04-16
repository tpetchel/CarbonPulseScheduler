using CarbonPulseScheduler.Api.Models;

namespace CarbonPulseScheduler.Api.Services;

public class DummyScheduler : IJobScheduler
{
    public Task<SchedulingDecision> RecommendAsync(Job job, SchedulingContext context)
    {
        // Simple heuristic: pick the midpoint of the allowable window
        var windowDuration = job.LatestEnd - job.EarliestStart - job.Duration;
        var midOffset = windowDuration / 2;
        var start = job.EarliestStart + midOffset;

        return Task.FromResult(new SchedulingDecision
        {
            RecommendedStart = start,
            Rationale = "Midpoint of allowable window (dummy scheduler)"
        });
    }
}
