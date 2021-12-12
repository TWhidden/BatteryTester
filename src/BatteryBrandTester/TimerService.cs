using System.Diagnostics;
using BatteryBrandTester.Events;
using PubSub;

namespace BatteryBrandTester;

public class TimerService : BackgroundService

{
    private readonly Stopwatch _stopwatch = new();
    private Timer _timer;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Hub.Default.Subscribe<RpmEvent>(OnRpm);
        _timer = new Timer(UpdateTime, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));
        return Task.CompletedTask;
    }

    private void UpdateTime(object? state)
    {
        Hub.Default.Publish(new TimeEvent(_stopwatch.Elapsed));
    }

    private void OnRpm(RpmEvent e)
    {
        if (e.Rpm == 0)
        {
            _stopwatch.Stop();
            Hub.Default.Publish(new TimeHistoryEvent(_stopwatch.Elapsed));
        }
        else
        {
            if (!_stopwatch.IsRunning) _stopwatch.Restart();
        }
    }
}