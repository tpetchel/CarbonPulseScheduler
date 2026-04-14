using CarbonPulseScheduler.Api.Models;
using CarbonPulseScheduler.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarbonPulseScheduler.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ForecastController : ControllerBase
{
    private readonly ICarbonIntensityProvider _provider;

    public ForecastController(ICarbonIntensityProvider provider)
    {
        _provider = provider;
    }

    [HttpGet]
    public IActionResult Get([FromQuery] string region, [FromQuery] DateTimeOffset start, [FromQuery] DateTimeOffset end)
    {
        if (string.IsNullOrWhiteSpace(region))
            return BadRequest("Region is required.");

        var forecast = _provider.GetForecast(region, start, end);
        return Ok(forecast);
    }
}
