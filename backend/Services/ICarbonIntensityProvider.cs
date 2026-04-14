using CarbonPulseScheduler.Api.Models;

namespace CarbonPulseScheduler.Api.Services;

public interface ICarbonIntensityProvider
{
    IReadOnlyList<CarbonIntensityPoint> GetForecast(string region, DateTimeOffset start, DateTimeOffset end);
}
