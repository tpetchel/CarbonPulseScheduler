using CarbonPulseScheduler.Api.Models;
using Microsoft.Extensions.Logging;

namespace CarbonPulseScheduler.Api.Services;

public class CarbonAwareScheduler : IJobScheduler
{
    private readonly ICarbonIntensityProvider _carbonProvider;
    private readonly ILogger<CarbonAwareScheduler> _logger;

    public CarbonAwareScheduler(ICarbonIntensityProvider carbonProvider, ILogger<CarbonAwareScheduler> logger)
    {
        _carbonProvider = carbonProvider;
        _logger = logger;
    }

    public async Task<SchedulingDecision> RecommendAsync(Job job, SchedulingContext context)
    {
        var forecast = context.Forecast;
        if (forecast.Count == 0)
        {
            forecast = await _carbonProvider.GetForecastAsync(job.Region, job.EarliestStart, job.LatestEnd);
        }

        if (forecast.Count == 0)
        {
            // No forecast available — fall back to earliest start
            return new SchedulingDecision
            {
                RecommendedStart = job.EarliestStart,
                Rationale = "No carbon forecast available; scheduled at earliest start"
            };
        }

        // Find the window of Duration length with the lowest average carbon intensity
        var bestStart = job.EarliestStart;
        var bestAvgIntensity = double.MaxValue;

        var latestPossibleStart = job.LatestEnd - job.Duration;

        _logger.LogInformation("Scheduler: EarliestStart={EarliestStart}, LatestEnd={LatestEnd}, Duration={Duration}, LatestPossibleStart={LatestPossibleStart}",
            job.EarliestStart, job.LatestEnd, job.Duration, latestPossibleStart);
        _logger.LogInformation("Scheduler: Forecast has {Count} points, range {First} to {Last}",
            forecast.Count, forecast[0].Timestamp, forecast[^1].Timestamp);

        var candidatesEvaluated = 0;
        foreach (var candidate in forecast)
        {
            if (candidate.Timestamp < job.EarliestStart || candidate.Timestamp > latestPossibleStart)
                continue;

            candidatesEvaluated++;
            var windowEnd = candidate.Timestamp + job.Duration;
            var windowPoints = forecast
                .Where(p => p.Timestamp >= candidate.Timestamp && p.Timestamp < windowEnd)
                .ToList();

            if (windowPoints.Count == 0)
                continue;

            var avg = windowPoints.Average(p => p.Intensity);
            if (avg < bestAvgIntensity)
            {
                bestAvgIntensity = avg;
                bestStart = candidate.Timestamp;
            }
        }

        _logger.LogInformation("Scheduler: Evaluated {Count} candidates, bestStart={BestStart}, bestAvg={BestAvg}",
            candidatesEvaluated, bestStart, bestAvgIntensity);

        return new SchedulingDecision
        {
            RecommendedStart = bestStart,
            Rationale = $"Lowest forecasted carbon intensity (avg {bestAvgIntensity:F1} gCO2/kWh)"
        };
    }
}
