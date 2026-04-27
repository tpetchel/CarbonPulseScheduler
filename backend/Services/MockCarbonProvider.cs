using CarbonPulseScheduler.Api.Models;

namespace CarbonPulseScheduler.Api.Services;

public class MockCarbonProvider : ICarbonIntensityProvider
{
    private static readonly IReadOnlyList<string> MockRegions =
    [
        "westus", "eastus", "northeurope", "westeurope",
        "eastasia", "australiaeast", "brazilsouth", "japaneast"
    ];

    public Task<IReadOnlyList<string>> GetRegionsAsync() =>
        Task.FromResult(MockRegions);

    public Task<IReadOnlyList<CarbonIntensityPoint>> GetForecastAsync(string region, DateTimeOffset start, DateTimeOffset end)
    {
        // Generate a synthetic multi-frequency curve with region-based offset.
        // Uses a deterministic hash (not string.GetHashCode which is randomized per process).
        var points = new List<CarbonIntensityPoint>();
        var regionSeed = DeterministicHash(region);
        var rng = new Random(regionSeed);
        var phaseOffset = rng.NextDouble() * Math.PI * 2;

        var current = start;
        while (current <= end)
        {
            var hourOfDay = current.Hour + current.Minute / 60.0;

            // 24-hour cycle: broad daily pattern (higher during day, lower at night)
            var dailyCycle = Math.Sin((hourOfDay - 6) * Math.PI / 12.0 + phaseOffset);
            // 4-hour cycle: creates multiple valleys within typical job windows
            var shortCycle = 0.5 * Math.Sin(hourOfDay * Math.PI / 2.0 + phaseOffset * 1.7);
            // 1.5-hour ripple: fine-grained variation for visual interest
            var ripple = 0.2 * Math.Sin(hourOfDay * Math.PI / 0.75 + phaseOffset * 0.6);

            var combined = dailyCycle + shortCycle + ripple;
            // Intensity between ~30 and ~450 gCO2/kWh (abstract units)
            var intensity = 250 + 120 * combined + rng.NextDouble() * 30 - 15;
            intensity = Math.Max(30, intensity);

            points.Add(new CarbonIntensityPoint
            {
                Timestamp = current,
                Intensity = Math.Round(intensity, 1)
            });

            current = current.AddMinutes(5);
        }

        return Task.FromResult<IReadOnlyList<CarbonIntensityPoint>>(points);
    }

    /// <summary>
    /// Simple deterministic string hash (FNV-1a) so the curve is stable across process restarts.
    /// </summary>
    private static int DeterministicHash(string s)
    {
        unchecked
        {
            uint hash = 2166136261;
            foreach (var c in s)
            {
                hash ^= c;
                hash *= 16777619;
            }
            return (int)hash;
        }
    }
}
