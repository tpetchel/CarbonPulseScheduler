using CarbonPulseScheduler.Api.Models;

namespace CarbonPulseScheduler.Api.Services;

public interface ICarbonIntensityProvider
{
    Task<IReadOnlyList<CarbonIntensityPoint>> GetForecastAsync(string region, DateTimeOffset start, DateTimeOffset end);
    Task<IReadOnlyList<string>> GetRegionsAsync();
}
