using CarbonPulseScheduler.Api.Models;

namespace CarbonPulseScheduler.Api.Services;

public interface IJobRepository
{
    Job Create(Job job);
    Job? Get(Guid jobId);
    IReadOnlyList<Job> List();
    void Update(Job job);
    void Delete(Guid jobId);
}
