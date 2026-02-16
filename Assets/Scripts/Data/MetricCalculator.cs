using System;
using System.Collections.Generic;
using System.Globalization;

public class MetricCalculator
{
    public List<UserStateData> Calculate(InputDataStore inputDataStore, DateTime asOfDate)
    {
        var result = new List<UserStateData>();
        if (inputDataStore == null)
        {
            return result;
        }

        var users = inputDataStore.Users ?? new List<UsersData>();
        var events = inputDataStore.ActivityEvents ?? new List<ActivityEventsData>();

        var asOf = asOfDate.Date;
        var start7d = asOf.AddDays(-6);

        foreach (var user in users)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.UserId))
            {
                continue;
            }

            var state = new UserStateData
            {
                UserId = user.UserId,
                Name = user.Name,
                City = user.City
            };

            for (var i = 0; i < events.Count; i++)
            {
                var evt = events[i];
                if (evt == null || !string.Equals(evt.UserId, user.UserId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!TryParseEventDate(evt.Date, out var eventDate))
                {
                    continue;
                }

                var date = eventDate.Date;
                if (date == asOf)
                {
                    state.MessagesToday += evt.Messages;
                    state.ReactionsToday += evt.Reactions;
                    state.UniqueGroupsToday += evt.UniqueGroups;
                }

                if (date >= start7d && date <= asOf)
                {
                    state.Messages7d += evt.Messages;
                    state.Reactions7d += evt.Reactions;
                }
            }

            result.Add(state);
        }

        result.Sort((a, b) => string.Compare(a.UserId, b.UserId, StringComparison.OrdinalIgnoreCase));
        return result;
    }

    public DateTime ResolveAsOfDate(InputDataStore inputDataStore, string explicitAsOfDate)
    {
        if (!string.IsNullOrWhiteSpace(explicitAsOfDate) &&
            DateTime.TryParseExact(explicitAsOfDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedExplicit))
        {
            return parsedExplicit.Date;
        }

        var latest = DateTime.MinValue;
        var hasLatest = false;
        var events = inputDataStore != null ? inputDataStore.ActivityEvents : null;

        if (events != null)
        {
            for (var i = 0; i < events.Count; i++)
            {
                var evt = events[i];
                if (evt == null || !TryParseEventDate(evt.Date, out var eventDate))
                {
                    continue;
                }

                var date = eventDate.Date;
                if (!hasLatest || date > latest)
                {
                    latest = date;
                    hasLatest = true;
                }
            }
        }

        return hasLatest ? latest : DateTime.Today;
    }

    private bool TryParseEventDate(string rawDate, out DateTime parsedDate)
    {
        if (DateTime.TryParseExact(rawDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
        {
            return true;
        }

        return DateTime.TryParse(rawDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
    }
}
