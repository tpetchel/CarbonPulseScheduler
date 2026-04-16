using CarbonPulseScheduler.Api.Models;

namespace CarbonPulseScheduler.Api.Services;

public interface IJobScheduler
{
    Task<SchedulingDecision> RecommendAsync(Job job, SchedulingContext context);
}
