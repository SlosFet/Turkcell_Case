using System;
using System.Globalization;

public static class RandomTimestampGenerator
{
    private static readonly Random Random = new Random();
    private static readonly object Sync = new object();

    public static string CreateIsoUtc(DateTime date)
    {
        int hour;
        int minute;
        int second;

        lock (Sync)
        {
            hour = Random.Next(0, 24);
            minute = Random.Next(0, 60);
            second = Random.Next(0, 60);
        }

        var timestamp = new DateTime(
            date.Year,
            date.Month,
            date.Day,
            hour,
            minute,
            second,
            DateTimeKind.Utc);

        return timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
    }
}
