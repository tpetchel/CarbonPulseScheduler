using CarbonPulseScheduler.Api.Models;
using CarbonPulseScheduler.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarbonPulseScheduler.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClockController : ControllerBase
{
    private readonly IVirtualClock _clock;

    public ClockController(IVirtualClock clock)
    {
        _clock = clock;
    }

    [HttpGet]
    public IActionResult GetClock() => Ok(new ClockResponse
    {
        VirtualNow = _clock.Now,
        SpeedMultiplier = _clock.SpeedMultiplier,
        Paused = _clock.Paused
    });

    [HttpPost]
    public IActionResult SetClock([FromBody] ClockRequest request)
    {
        switch (request.Mode?.ToLowerInvariant())
        {
            case "set":
                if (DateTimeOffset.TryParse(request.Value, out var dt))
                    _clock.SetTime(dt);
                else
                    return BadRequest("Invalid datetime value.");
                break;

            case "reset":
                _clock.Reset();
                break;

            case "accelerate":
                if (double.TryParse(request.Value, out var speed) && speed > 0)
                    _clock.SetSpeed(speed);
                else
                    return BadRequest("Invalid speed value.");
                break;

            case "pause":
                _clock.SetPaused(true);
                break;

            case "resume":
                _clock.SetPaused(false);
                break;

            default:
                return BadRequest("Mode must be one of: set, reset, accelerate, pause, resume.");
        }

        return Ok(new ClockResponse
        {
            VirtualNow = _clock.Now,
            SpeedMultiplier = _clock.SpeedMultiplier,
            Paused = _clock.Paused
        });
    }
}
