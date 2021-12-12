using System.Text;

namespace BatteryBrandTester.Extensions;

public static class TimeSpanExtensions
{
    public static string TimeSpanToHuman(this TimeSpan timespan)
    {
        var x = (long)timespan.TotalSeconds;

        var seconds = x % 60;
        x /= 60;
        var minutes = x % 60;
        x /= 60;
        var hours = x % 24;
        x /= 24;
        var days = x;

        var l = new List<string>();

        if (days > 1)
            l.Add($"{days} Days");
        else if (days == 1)
            l.Add($"{days} Day");

        if (hours > 1)
            l.Add($"{hours} Hours");
        else if (hours == 1)
            l.Add($"{hours} Hour");

        if (minutes > 1)
            l.Add($"{minutes} Minutes");
        else if (minutes == 1)
            l.Add($"{minutes} Minute");

        if (seconds > 1)
            l.Add($"{seconds} Seconds");
        else if (seconds == 1)
            l.Add($"{seconds} Second");

        return string.Join(", ", l);
    }

    public static string TimeSpanToHumanShort(this TimeSpan t, bool includeMs = false)
    {
        if (t.TotalSeconds < 1 && !includeMs) return "0s";

        var sb = new StringBuilder();
        if (t.Days > 0) sb.Append($"{t.Days}d");
        if (t.Hours > 0) sb.Append($"{t.Hours}h");
        if (t.Minutes > 0) sb.Append($"{t.Minutes}m");
        if (t.Seconds > 0) sb.Append($"{t.Seconds}s");
        if (t.Milliseconds > 0 && includeMs) sb.Append($"{t.Milliseconds}ms");
        return sb.ToString();
    }
}