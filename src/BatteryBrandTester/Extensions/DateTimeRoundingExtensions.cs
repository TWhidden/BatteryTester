namespace BatteryBrandTester.Extensions;

public static class DateTimeRoundingExtensions
{
    public static DateTime RoundUp(this DateTime dt, TimeSpan d)
    {
        var delta = (d.Ticks - dt.Ticks % d.Ticks) % d.Ticks;
        return new DateTime(dt.Ticks + delta, dt.Kind);
    }

    public static DateTime RoundDown(this DateTime dt, TimeSpan d)
    {
        var delta = dt.Ticks % d.Ticks;
        return new DateTime(dt.Ticks - delta, dt.Kind);
    }

    public static DateTime RoundToNearest(this DateTime dt, TimeSpan d)
    {
        var delta = dt.Ticks % d.Ticks;
        var roundUp = delta > d.Ticks / 2;

        return roundUp ? dt.RoundUp(d) : dt.RoundDown(d);
    }

    public static DateTime? RoundUp(this DateTime? dt, TimeSpan d)
    {
        if (!dt.HasValue) return null;
        return RoundUp(DateTime.MinValue, d);
    }

    public static DateTime? RoundDown(this DateTime? dt, TimeSpan d)
    {
        if (!dt.HasValue) return null;
        return RoundDown(dt.Value, d);
    }

    public static DateTime? RoundToNearest(this DateTime? dt, TimeSpan d)
    {
        if (!dt.HasValue) return null;
        return RoundToNearest(dt.Value, d);
    }
}