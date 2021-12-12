using BatteryBrandTester.Events;
using BatteryBrandTester.Extensions;
using BatteryBrandTester.Helpers;
using OledI2cCore;
using PubSub;

namespace BatteryBrandTester;

public class ScreenService : BackgroundService

{
    private int _maxStringCount = -1;
    private int _maxStringRpm = -1;
    private int _maxStringTime = -1;
    private int _maxStringHistory = -1;
    private readonly OledCore _screen;
    private Timer _timer;
    private bool _writing;
    private List<(TimeSpan tm, int count)> _timeHistory = new();
    private int _lastCount = 0;

    public ScreenService()
    {
        var logger = new PiLogger();
        logger.Info("Creating I2C...");
        var i2C = new PiI2CCore();

        logger.Info("Create Oled...");

        // Create the Oled Object, with the wrapper for the I2C and the logger.
        // Set the defaults for the testing screen used
        _screen = new OledCore(i2C, 128, 64, logger: logger,
            screenDriver: ScreenDriver.SH1106);

        logger.Info("Init Oled...");
        // Init the Oled (setup params, clear display, etc)
        _screen.Initialise();

        _timer = new Timer(UpdateScreen, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));
    }

    private void UpdateScreen(object? state)
    {
        if (_writing) return;

        try
        {
            _writing = true;
            _screen.UpdateDirtyBytes();
        }
        finally
        {
            _writing = false;
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Hub.Default.Subscribe<RotationCountEvent>(OnRotationCount);
        Hub.Default.Subscribe<RpmEvent>(OnRpm);
        Hub.Default.Subscribe<TimeEvent>(OnTime);
        Hub.Default.Subscribe<TimeHistoryEvent>(OnTimeHistoryEvent);
        return Task.CompletedTask;
    }


    private void OnTimeHistoryEvent(TimeHistoryEvent e)
    {
        _timeHistory.Add(new(e.RanFor, _lastCount));

        const int maxShow = 3;

        var lowestIndex = _timeHistory.Count - maxShow;

        lowestIndex = Math.Max(0, lowestIndex);

        var startingYOffset = 30;
        var lineHeight = 10;

        var c = 0;
        for (var index = _timeHistory.Count-1; index >= lowestIndex; index--)
        {
            var history = _timeHistory[index];
            var t = $"{index+1}: {history.tm.TimeSpanToHumanShort()} {history.count}" ;
            SetMaxString(ref _maxStringHistory, t);
            _screen.WriteString(0, (byte)(startingYOffset + (lineHeight*c)), t, charMax: _maxStringHistory);
            c++;
        }
    }

    private void OnTime(TimeEvent e)
    {
        var str = $"E: {e.Time.TimeSpanToHumanShort()}";
        SetMaxString(ref _maxStringTime, str);
        _screen.WriteString(0, 20, str, charMax: _maxStringTime);
    }

    private void OnRotationCount(RotationCountEvent e)
    {
        _lastCount = e.RotationCount;
        var str = $"C: {e.RotationCount}";
        SetMaxString(ref _maxStringCount, str);
        _screen.WriteString(0, 0, str, charMax: _maxStringCount);
    }

    private void OnRpm(RpmEvent e)
    {
        var str = $"RPM: {e.Rpm}";
        SetMaxString(ref _maxStringRpm, str);
        _screen.WriteString(64, 0, str, charMax: _maxStringRpm);
    }

    private void SetMaxString(ref int maxValue, string newString)
    {
        if (newString.Length > maxValue) maxValue = newString.Length;
    }
}