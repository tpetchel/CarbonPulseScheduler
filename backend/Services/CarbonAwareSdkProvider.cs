using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CarbonPulseScheduler.Api.Models;

namespace CarbonPulseScheduler.Api.Services;

public class CarbonAwareSdkOptions
{
    public string BaseUrl { get; set; } = "http://localhost:8080";
}

public class CarbonAwareSdkProvider : ICarbonIntensityProvider
{
    private readonly HttpClient _http;
    private readonly ILogger<CarbonAwareSdkProvider> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CarbonAwareSdkProvider(HttpClient http, ILogger<CarbonAwareSdkProvider> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<IReadOnlyList<string>> GetRegionsAsync()
    {
        try
        {
            var response = await _http.GetAsync("/locations");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Carbon Aware SDK /locations returned {StatusCode}", (int)response.StatusCode);
                return [];
            }

            var locations = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>(JsonOptions);
            return locations?.Keys.Order().ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch regions from Carbon Aware SDK");
            return [];
        }
    }

    public async Task<IReadOnlyList<CarbonIntensityPoint>> GetForecastAsync(
        string region, DateTimeOffset start, DateTimeOffset end)
    {
        // Call the Carbon Aware SDK WebAPI forecast endpoint
        var url = $"/emissions/forecasts/current?location={Uri.EscapeDataString(region)}" +
                  $"&dataStartAt={Uri.EscapeDataString(start.UtcDateTime.ToString("O"))}" +
                  $"&dataEndAt={Uri.EscapeDataString(end.UtcDateTime.ToString("O"))}";

        _logger.LogInformation("Fetching carbon forecast from SDK: {Url}", url);

        HttpResponseMessage response;
        try
        {
            response = await _http.GetAsync(url);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to reach Carbon Aware SDK for region {Region}", region);
            return [];
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Carbon Aware SDK returned {StatusCode} for region {Region}. Returning empty forecast.",
                (int)response.StatusCode, region);
            return [];
        }

        var rawJson = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Raw SDK response (first 2000 chars): {Raw}", rawJson.Length > 2000 ? rawJson[..2000] : rawJson);

        var forecasts = JsonSerializer.Deserialize<List<SdkForecastResponse>>(rawJson, JsonOptions);

        if (forecasts is null || forecasts.Count == 0)
        {
            _logger.LogWarning("No forecast data returned from Carbon Aware SDK for region {Region}", region);
            return [];
        }

        // The SDK returns forecasts as an array; take the first (most recent) forecast
        var forecast = forecasts[0];
        _logger.LogInformation("Forecast has {OptimalCount} optimal points, {DataCount} forecast data points",
            forecast.OptimalDataPoints.Count, forecast.ForecastData?.Count ?? 0);

        if (forecast.OptimalDataPoints.Count > 0)
        {
            var first = forecast.OptimalDataPoints[0];
            _logger.LogInformation("First optimal point: Timestamp={Timestamp}, Value={Value}, Location={Location}, Duration={Duration}",
                first.Timestamp, first.Value, first.Location, first.Duration);
        }

        if (forecast.ForecastData is { Count: > 0 })
        {
            var first = forecast.ForecastData[0];
            var last = forecast.ForecastData[^1];
            _logger.LogInformation("First data point: Timestamp={Timestamp}, Value={Value}", first.Timestamp, first.Value);
            _logger.LogInformation("Last data point: Timestamp={Timestamp}, Value={Value}", last.Timestamp, last.Value);
        }

        var result = forecast.OptimalDataPoints
            .Concat(forecast.ForecastData ?? [])
            .DistinctBy(p => p.Timestamp)
            .OrderBy(p => p.Timestamp)
            .Select(p => new CarbonIntensityPoint
            {
                Timestamp = p.Timestamp,
                Intensity = p.Value
            })
            .ToList();

        _logger.LogInformation("Returning {Count} carbon intensity points", result.Count);
        return result;
    }

    // DTOs matching the Carbon Aware SDK WebAPI response shape
    private class SdkForecastResponse
    {
        public DateTimeOffset GeneratedAt { get; set; }
        public List<SdkDataPoint> OptimalDataPoints { get; set; } = [];
        public List<SdkDataPoint>? ForecastData { get; set; }
    }

    private class SdkDataPoint
    {
        public string Location { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; }
        public double Value { get; set; }
        public int Duration { get; set; }
    }
}
