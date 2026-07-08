namespace OctopusData.Helpers;

public static class DateHelper
{
    public static string IsoDateOnly(DateTime value)
    {
        return $"{value:yyyy-MM-dd}";
    }

    public static string LogFileSuffix()
    {
        return $"{DateTime.Now:yyyy-MM-dd HH-mm-ss.fff}";
    }

    public static string LogFileSuffix(string suffix)
    {
        return $"{DateTime.Now:yyyy-MM-dd} {suffix}";
    }

    public static string LogEntryTimestamp()
    {
        return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}";
    }

    public static string StartOfToday(DateTime date)
    {
        var day = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
        return $"{day:yyyy-MM-ddTHH:mm:ss.fffZ}";
    }

    public static string StartOfTomorrow(DateTime date)
    {
        var day = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
        day = day.AddDays(1);
        return $"{day:yyyy-MM-ddTHH:mm:ss.fffZ}";
    }

    public static string FirstDayOfThisMonth(DateTime date)
    {
        var day = new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        return $"{day:yyyy-MM-ddTHH:mm:ss.fffZ}";
    }

    public static string FirstDayOfNextMonth(DateTime date, bool startOfDay = true)
    {
        var day = new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        day = day.AddMonths(1);
        if (!startOfDay)
        {
            day = day.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);
        }
        return $"{day:yyyy-MM-ddTHH:mm:ss.fffZ}";
    }
}