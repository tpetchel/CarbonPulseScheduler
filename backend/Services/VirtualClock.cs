namespace CarbonPulseScheduler.Api.Services;

public class VirtualClock : IVirtualClock
{
    private readonly object _lock = new();
    private DateTimeOffset _anchorVirtual;
    private DateTimeOffset _anchorReal;
    private double _speed = 1.0;
    private bool _paused;

    public VirtualClock()
    {
        _anchorVirtual = DateTimeOffset.UtcNow;
        _anchorReal = DateTimeOffset.UtcNow;
    }

    public DateTimeOffset Now
    {
        get
        {
            lock (_lock)
            {
                if (_paused)
                    return _anchorVirtual;

                var realElapsed = DateTimeOffset.UtcNow - _anchorReal;
                return _anchorVirtual + realElapsed * _speed;
            }
        }
    }

    public double SpeedMultiplier
    {
        get { lock (_lock) return _speed; }
    }

    public bool Paused
    {
        get { lock (_lock) return _paused; }
    }

    public void SetTime(DateTimeOffset time)
    {
        lock (_lock)
        {
            _anchorVirtual = time;
            _anchorReal = DateTimeOffset.UtcNow;
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _anchorVirtual = DateTimeOffset.UtcNow;
            _anchorReal = DateTimeOffset.UtcNow;
            _speed = 1.0;
            _paused = false;
        }
    }

    public void SetSpeed(double multiplier)
    {
        lock (_lock)
        {
            // Snapshot current virtual time before changing speed
            _anchorVirtual = Now;
            _anchorReal = DateTimeOffset.UtcNow;
            _speed = multiplier;
        }
    }

    public void SetPaused(bool paused)
    {
        lock (_lock)
        {
            if (paused && !_paused)
            {
                // Snapshot virtual time when pausing
                _anchorVirtual = Now;
                _anchorReal = DateTimeOffset.UtcNow;
            }
            else if (!paused && _paused)
            {
                // Resume from where we paused
                _anchorReal = DateTimeOffset.UtcNow;
            }
            _paused = paused;
        }
    }
}
