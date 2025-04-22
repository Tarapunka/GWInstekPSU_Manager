namespace GWInstekPSUManager.Core.Models;

public class ChannelTimeTracker
{
    private DateTime _lastStartTime;
    private TimeSpan _totalElapsed;
    private bool _isRunning;

    public TimeSpan ElapsedTime => _isRunning
        ? _totalElapsed + (DateTime.Now - _lastStartTime)
        : _totalElapsed;

    public DateTime LastStartTime => _isRunning ? _lastStartTime : DateTime.MinValue;

    public bool IsRunning => _isRunning;

    public void Start()
    {
        if (!_isRunning)
        {
            _lastStartTime = DateTime.Now;
            _isRunning = true;
        }
    }

    public void Stop()
    {
        if (_isRunning)
        {
            _totalElapsed += DateTime.Now - _lastStartTime;
            _isRunning = false;
        }
    }

    public void Reset()
    {
        _totalElapsed = TimeSpan.Zero;
        _isRunning = false;
    }
}