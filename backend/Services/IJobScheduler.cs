using CarbonPulseScheduler.Api.Models;

namespace CarbonPulseScheduler.Api.Services;

public interface IJobScheduler
{
    SchedulingDecision Recommend(Job job, SchedulingContext context);
}
