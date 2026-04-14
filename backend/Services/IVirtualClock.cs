namespace CarbonPulseScheduler.Api.Services;

public interface IVirtualClock
{
    DateTimeOffset Now { get; }
    double SpeedMultiplier { get; }
    bool Paused { get; }
    void SetTime(DateTimeOffset time);
    void Reset();
    void SetSpeed(double multiplier);
    void SetPaused(bool paused);
}
