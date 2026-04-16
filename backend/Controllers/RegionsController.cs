using CarbonPulseScheduler.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarbonPulseScheduler.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegionsController : ControllerBase
{
    private readonly ICarbonIntensityProvider _provider;
    private readonly IConfiguration _config;

    public RegionsController(ICarbonIntensityProvider provider, IConfiguration config)
    {
        _provider = provider;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var regions = await _provider.GetRegionsAsync();

        // If AllowedRegions is configured, filter to only those
        var allowed = _config.GetSection("AllowedRegions").Get<string[]>();
        if (allowed is { Length: > 0 })
        {
            var allowedSet = new HashSet<string>(allowed, StringComparer.OrdinalIgnoreCase);
            regions = regions.Where(r => allowedSet.Contains(r)).ToList();
        }

        return Ok(regions);
    }
}
