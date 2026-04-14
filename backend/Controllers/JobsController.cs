using CarbonPulseScheduler.Api.Models;
using CarbonPulseScheduler.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarbonPulseScheduler.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IJobRepository _repo;
    private readonly IJobScheduler _scheduler;
    private readonly ICarbonIntensityProvider _carbonProvider;

    public JobsController(IJobRepository repo, IJobScheduler scheduler, ICarbonIntensityProvider carbonProvider)
    {
        _repo = repo;
        _scheduler = scheduler;
        _carbonProvider = carbonProvider;
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateJobRequest request)
    {
        if (request.DurationMinutes <= 0)
            return BadRequest("Duration must be positive.");

        var duration = TimeSpan.FromMinutes(request.DurationMinutes);
        if (request.LatestEnd - request.EarliestStart < duration)
            return BadRequest("The allowable window is shorter than the job duration.");

        var job = new Job
        {
            Region = request.Region,
            EarliestStart = request.EarliestStart,
            LatestEnd = request.LatestEnd,
            Duration = duration,
            Status = JobStatus.Pending
        };

        var forecast = _carbonProvider.GetForecast(job.Region, job.EarliestStart, job.LatestEnd);
        var context = new SchedulingContext { Forecast = forecast };
        var decision = _scheduler.Recommend(job, context);

        job.ScheduledStart = decision.RecommendedStart;
        job.ScheduledEnd = decision.RecommendedStart + job.Duration;
        job.Rationale = decision.Rationale;
        job.Status = JobStatus.Scheduled;

        _repo.Create(job);

        return Ok(JobResponse.FromJob(job));
    }

    [HttpGet("{jobId:guid}")]
    public IActionResult Get(Guid jobId)
    {
        var job = _repo.Get(jobId);
        if (job is null) return NotFound();
        return Ok(JobResponse.FromJob(job));
    }

    [HttpGet]
    public IActionResult List() => Ok(_repo.List().Select(JobResponse.FromJob));

    [HttpPost("{jobId:guid}/cancel")]
    public IActionResult Cancel(Guid jobId)
    {
        var job = _repo.Get(jobId);
        if (job is null) return NotFound();
        if (job.Status is JobStatus.Completed or JobStatus.Cancelled)
            return BadRequest($"Cannot cancel a job with status {job.Status}.");

        job.Status = JobStatus.Cancelled;
        _repo.Update(job);
        return Ok(JobResponse.FromJob(job));
    }
}
