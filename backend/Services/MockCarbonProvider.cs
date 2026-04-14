using CarbonPulseScheduler.Api.Models;

namespace CarbonPulseScheduler.Api.Services;

public class MockCarbonProvider : ICarbonIntensityProvider
{
    public IReadOnlyList<CarbonIntensityPoint> GetForecast(string region, DateTimeOffset start, DateTimeOffset end)
    {
        // Generate a synthetic sinusoidal curve with region-based offset
        var points = new List<CarbonIntensityPoint>();
        var regionSeed = region.GetHashCode();
        var rng = new Random(regionSeed);
        var phaseOffset = rng.NextDouble() * Math.PI * 2;

        var current = start;
        while (current <= end)
        {
            var hoursFromStart = (current - start).TotalHours;
            // Base sinusoidal pattern: higher carbon during day (peak ~14:00), lower at night
            var hourOfDay = current.Hour + current.Minute / 60.0;
            var dailyCycle = Math.Sin((hourOfDay - 6) * Math.PI / 12.0 + phaseOffset);
            // Intensity between 50 and 450 gCO2/kWh (abstract units)
            var intensity = 250 + 200 * dailyCycle + rng.NextDouble() * 30 - 15;
            intensity = Math.Max(30, intensity);

            points.Add(new CarbonIntensityPoint
            {
                Timestamp = current,
                Intensity = Math.Round(intensity, 1)
            });

            current = current.AddMinutes(5);
        }

        return points;
    }
}
