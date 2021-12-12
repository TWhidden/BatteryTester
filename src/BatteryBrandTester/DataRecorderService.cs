using System.Text;
using BatteryBrandTester.Events;
using BatteryBrandTester.Extensions;
using PubSub;

namespace BatteryBrandTester;

public class DataRecorderService : BackgroundService
{
    private readonly List<DateTime> _rotations = new();
    private int _lastCount;
    private TimeSpan _lastTime = TimeSpan.Zero;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Hub.Default.Subscribe<RpmEvent>(OnRpmEvent);
        Hub.Default.Subscribe<RotationEvent>(OnRotationEvent);
        Hub.Default.Subscribe<TimeEvent>(OnTimeEvent);
        Hub.Default.Subscribe<RotationCountEvent>(OnRotationCountEvent);
        return Task.CompletedTask;
    }

    private void OnRotationCountEvent(RotationCountEvent obj)
    {
        _lastCount = obj.RotationCount;
    }

    private void OnTimeEvent(TimeEvent obj)
    {
        _lastTime = obj.Time;
    }

    private void OnRotationEvent(RotationEvent obj)
    {
        _rotations.Add(DateTime.Now);
    }

    private void OnRpmEvent(RpmEvent obj)
    {
        if (obj.Rpm == 0)
            if (_rotations.Count > 0)
            {
                var reportTime = DateTime.Now;
                // Write the data
                var fileName = $"B-Raw-{reportTime:O}.csv";
                var fileData = string.Join(Environment.NewLine, _rotations);
                File.WriteAllText(fileName, fileData);

                // ********************************************

                // Write a summary just incase it was not recorded
                var sb = new StringBuilder();
                sb.AppendLine($"Run Time: {_lastTime.TimeSpanToHumanShort()}");
                sb.AppendLine($"Total Rotations: {_lastCount}");
                fileName = $"B-Stat-{reportTime:O}.csv";
                File.WriteAllText(fileName, sb.ToString());

                // ********************************************

                // Write RPS breakdown
                fileName = $"B-RPMs-{reportTime:O}.csv";
                // create a rounded-to-second list
                var rounding = TimeSpan.FromSeconds(1);

                // Create a dictionary of key/values
                var roundedList = _rotations
                    .Select(x => x.RoundDown(rounding))
                    .GroupBy(x => x)
                    .ToDictionary(x => x.Key, x => x.Count());
                var currentTime = roundedList.Keys.First();
                var lastTime = roundedList.Keys.Last();

                sb.Clear();
                while (currentTime <= lastTime)
                {
                    // Build up the string
                    if (roundedList.TryGetValue(currentTime, out var count))
                        sb.AppendLine($"{currentTime}, {count * 60}");
                    else
                        sb.AppendLine($"{currentTime}, 0");

                    currentTime = currentTime.Add(rounding);
                }

                File.WriteAllText(fileName, sb.ToString());

                // **********************************

                // Write RPS breakdown
                fileName = $"B-RPM-{reportTime:O}.csv";
                // create a rounded-to-second list
                rounding = TimeSpan.FromMinutes(1);

                // Create a dictionary of key/values
                roundedList = _rotations
                    .Select(x => x.RoundDown(rounding))
                    .GroupBy(x => x)
                    .ToDictionary(x => x.Key, x => x.Count());
                currentTime = roundedList.Keys.First();
                lastTime = roundedList.Keys.Last();

                sb.Clear();
                while (currentTime <= lastTime)
                {
                    // Build up the string
                    if (roundedList.TryGetValue(currentTime, out var count))
                        sb.AppendLine($"{currentTime}, {count}");
                    else
                        sb.AppendLine($"{currentTime}, 0");

                    currentTime = currentTime.Add(rounding);
                }

                File.WriteAllText(fileName, sb.ToString());

                // Reset the Data
                _rotations.Clear();
                _lastCount = 0;
                _lastTime = TimeSpan.Zero;
            }
    }
}