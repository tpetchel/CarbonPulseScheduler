namespace CarbonPulseScheduler.Api.Models;

public class SchedulingContext
{
    public IReadOnlyList<CarbonIntensityPoint> Forecast { get; set; } = [];
}
