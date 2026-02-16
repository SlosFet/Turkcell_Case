using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

public class BadgeCalculator
{
    private static readonly Regex ConditionRegex = new Regex(
        "^total_points\\s*(>=|<=|==|>|<)\\s*(-?[0-9]+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public List<BadgeAwardsData> Calculate(List<BadgesData> badges, List<LeaderboardData> leaderboard, DateTime dataDate)
    {
        var result = new List<BadgeAwardsData>();
        badges = badges ?? new List<BadgesData>();

        if (leaderboard == null)
        {
            return result;
        }

        var orderedBadges = new List<BadgesData>();
        for (var i = 0; i < badges.Count; i++)
        {
            var badge = badges[i];
            if (badge == null || string.IsNullOrWhiteSpace(badge.BadgeId))
            {
                continue;
            }

            orderedBadges.Add(badge);
        }

        orderedBadges.Sort((a, b) =>
        {
            var levelCompare = a.Level.CompareTo(b.Level);
            if (levelCompare != 0)
            {
                return levelCompare;
            }

            return string.Compare(a.BadgeId, b.BadgeId, StringComparison.OrdinalIgnoreCase);
        });

        var orderedLeaderboard = new List<LeaderboardData>(leaderboard);
        orderedLeaderboard.Sort((a, b) => string.Compare(a.UserId, b.UserId, StringComparison.OrdinalIgnoreCase));

        var awardedAt = dataDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        for (var userIndex = 0; userIndex < orderedLeaderboard.Count; userIndex++)
        {
            var userRow = orderedLeaderboard[userIndex];
            if (userRow == null || string.IsNullOrWhiteSpace(userRow.UserId))
            {
                continue;
            }

            for (var badgeIndex = 0; badgeIndex < orderedBadges.Count; badgeIndex++)
            {
                var badge = orderedBadges[badgeIndex];
                if (!EvaluateCondition(badge.Condition, userRow.TotalPoints))
                {
                    continue;
                }

                result.Add(new BadgeAwardsData
                {
                    UserId = userRow.UserId,
                    BadgeId = badge.BadgeId,
                    AwardedAt = awardedAt
                });
            }
        }

        return result;
    }

    private bool EvaluateCondition(string condition, int totalPoints)
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            return false;
        }

        var match = ConditionRegex.Match(condition.Trim());
        if (!match.Success)
        {
            return false;
        }

        var op = match.Groups[1].Value;
        var rawTarget = match.Groups[2].Value;

        if (!int.TryParse(rawTarget, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target))
        {
            return false;
        }

        switch (op)
        {
            case ">=": return totalPoints >= target;
            case "<=": return totalPoints <= target;
            case "==": return totalPoints == target;
            case ">": return totalPoints > target;
            case "<": return totalPoints < target;
            default: return false;
        }
    }
}
