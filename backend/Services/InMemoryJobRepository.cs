using System.Collections.Concurrent;
using CarbonPulseScheduler.Api.Models;

namespace CarbonPulseScheduler.Api.Services;

public class InMemoryJobRepository : IJobRepository
{
    private readonly ConcurrentDictionary<Guid, Job> _jobs = new();

    public Job Create(Job job)
    {
        _jobs[job.JobId] = job;
        return job;
    }

    public Job? Get(Guid jobId) => _jobs.GetValueOrDefault(jobId);

    public IReadOnlyList<Job> List() => _jobs.Values.ToList();

    public void Update(Job job) => _jobs[job.JobId] = job;

    public void Delete(Guid jobId) => _jobs.TryRemove(jobId, out _);
}
