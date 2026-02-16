using System;
using System.Collections.Generic;

public class LeaderboardCalculator
{
    public List<LeaderboardData> Calculate(List<PointsLedgerData> pointsLedger)
    {
        var result = new List<LeaderboardData>();
        if (pointsLedger == null)
        {
            return result;
        }

        var totalsByUser = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < pointsLedger.Count; i++)
        {
            var entry = pointsLedger[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.UserId))
            {
                continue;
            }

            if (!totalsByUser.ContainsKey(entry.UserId))
            {
                totalsByUser[entry.UserId] = 0;
            }

            totalsByUser[entry.UserId] += entry.PointsDelta;
        }

        var rows = new List<LeaderboardData>();
        foreach (var pair in totalsByUser)
        {
            rows.Add(new LeaderboardData
            {
                UserId = pair.Key,
                TotalPoints = pair.Value
            });
        }

        rows.Sort((a, b) =>
        {
            var pointsCompare = b.TotalPoints.CompareTo(a.TotalPoints);
            if (pointsCompare != 0)
            {
                return pointsCompare;
            }

            return string.Compare(a.UserId, b.UserId, StringComparison.OrdinalIgnoreCase);
        });

        for (var i = 0; i < rows.Count; i++)
        {
            rows[i].Rank = i + 1;
        }

        return rows;
    }
}
