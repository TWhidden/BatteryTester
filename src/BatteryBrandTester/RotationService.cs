using BatteryBrandTester.Events;
using PubSub;

namespace BatteryBrandTester;

public class RotationService : BackgroundService
{
    private readonly object _collectionLock = new();
    private int _count;
    private int _lastRpm;
    private Timer _timer;

    private List<DateTime> _times = new();

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(TimerCallback, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

        Hub.Default.Subscribe<RotationEvent>(OnRotation);
        Hub.Default.Subscribe<RpmEvent>(OnRpmEvent);
        return Task.CompletedTask;
    }

    private void OnRpmEvent(RpmEvent obj)
    {
        if (obj.Rpm == 0)
            // reset the count to 0. Next publish will be zero start
            _count = 0;
    }

    private void TimerCallback(object? state)
    {
        CalcRPM();
    }

    private void OnRotation(RotationEvent e)
    {
        var c = Interlocked.Increment(ref _count);
        Hub.Default.Publish(new RotationCountEvent(c));
        lock (_collectionLock)
        {
            _times.Add(DateTime.Now);
        }
    }

    private void CalcRPM()
    {
        int count;
        lock (_collectionLock)
        {
            var t = _times.Where(x => x > DateTime.Now.AddSeconds(-1)).ToList();
            _times = t;
            count = _times.Count;
        }

        var rps = count;
        var rpm = rps * 60;
        if (_lastRpm != rpm)
        {
            _lastRpm = rpm;

            Hub.Default.Publish(new RpmEvent(rpm));
        }
    }
}