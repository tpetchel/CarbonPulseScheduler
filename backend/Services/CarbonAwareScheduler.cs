using CarbonPulseScheduler.Api.Models;

namespace CarbonPulseScheduler.Api.Services;

public class CarbonAwareScheduler : IJobScheduler
{
    private readonly ICarbonIntensityProvider _carbonProvider;

    public CarbonAwareScheduler(ICarbonIntensityProvider carbonProvider)
    {
        _carbonProvider = carbonProvider;
    }

    public SchedulingDecision Recommend(Job job, SchedulingContext context)
    {
        var forecast = context.Forecast;
        if (forecast.Count == 0)
        {
            forecast = _carbonProvider.GetForecast(job.Region, job.EarliestStart, job.LatestEnd);
        }

        // Find the window of Duration length with the lowest average carbon intensity
        var bestStart = job.EarliestStart;
        var bestAvgIntensity = double.MaxValue;

        var latestPossibleStart = job.LatestEnd - job.Duration;

        foreach (var candidate in forecast)
        {
            if (candidate.Timestamp < job.EarliestStart || candidate.Timestamp > latestPossibleStart)
                continue;

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

        return new SchedulingDecision
        {
            RecommendedStart = bestStart,
            Rationale = $"Lowest forecasted carbon intensity (avg {bestAvgIntensity:F1} gCO2/kWh)"
        };
    }
}
