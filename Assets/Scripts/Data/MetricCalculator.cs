using System;
using System.Collections.Generic;
using System.Globalization;

public class MetricCalculator
{
    public List<UserStateData> Calculate(InputDataStore inputDataStore)
    {
        var result = new List<UserStateData>();
        if (inputDataStore == null)
        {
            return result;
        }

        if (!TryResolveAsOfDate(inputDataStore, out var asOfDate))
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

    public bool TryResolveAsOfDate(InputDataStore inputDataStore, out DateTime asOfDate)
    {
        asOfDate = DateTime.MinValue;
        var events = inputDataStore != null ? inputDataStore.ActivityEvents : null;

        if (events == null || events.Count == 0)
        {
            return false;
        }

        var hasLatest = false;
        var latest = DateTime.MinValue;

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

        if (!hasLatest)
        {
            return false;
        }

        asOfDate = latest;
        return true;
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
