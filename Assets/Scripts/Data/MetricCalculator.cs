using System;
using System.Collections.Generic;

public class MetricCalculator
{
    public List<UserStateData> Calculate(List<UsersData> users, List<ActivityEventsData> activityEvents, DateTime dataDate)
    {
        var result = new List<UserStateData>();

        var asOf = dataDate.Date;
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

            for (var i = 0; i < activityEvents.Count; i++)
            {
                var evt = activityEvents[i];
                if (evt == null || !string.Equals(evt.UserId, user.UserId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!DateTime.TryParse(evt.Date, out var eventDate))
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
}
