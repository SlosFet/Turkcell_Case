using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

public class ChallengeCalculator
{
    private static readonly Regex ConditionRegex = new Regex(
        "^([a-zA-Z0-9_]+)\\s*(>=|<=|==|>|<)\\s*(-?[0-9]+)$",
        RegexOptions.Compiled);

    public List<ChallengeAwardsData> Calculate(InputDataStore inputDataStore, List<UserStateData> userState, DateTime dataDate)
    {
        var results = new List<ChallengeAwardsData>();
        var challenges = inputDataStore != null ? inputDataStore.Challenges : null;

        if (challenges == null || userState == null)
        {
            return results;
        }

        var activeChallenges = new List<ChallengesData>();
        for (var i = 0; i < challenges.Count; i++)
        {
            var challenge = challenges[i];
            if (challenge == null || !challenge.IsActive || string.IsNullOrWhiteSpace(challenge.ChallengeId))
            {
                continue;
            }

            activeChallenges.Add(challenge);
        }

        activeChallenges.Sort(CompareChallenges);

        var orderedUsers = new List<UserStateData>(userState);
        orderedUsers.Sort((a, b) => string.Compare(a.UserId, b.UserId, StringComparison.OrdinalIgnoreCase));

        var awardSequence = 100;
        for (var userIndex = 0; userIndex < orderedUsers.Count; userIndex++)
        {
            var user = orderedUsers[userIndex];
            if (user == null || string.IsNullOrWhiteSpace(user.UserId))
            {
                continue;
            }

            var triggered = new List<ChallengesData>();
            for (var c = 0; c < activeChallenges.Count; c++)
            {
                var challenge = activeChallenges[c];
                if (EvaluateCondition(challenge.Condition, user))
                {
                    triggered.Add(challenge);
                }
            }

            if (triggered.Count == 0)
            {
                continue;
            }

            var selected = triggered[0];
            var triggeredIds = JoinChallengeIds(triggered, 0);
            var suppressedIds = JoinChallengeIds(triggered, 1);

            results.Add(new ChallengeAwardsData
            {
                AwardId = "A-" + awardSequence.ToString(CultureInfo.InvariantCulture),
                UserId = user.UserId,
                TriggeredChallenges = triggeredIds,
                SelectedChallenge = selected.ChallengeId,
                RewardPoints = selected.RewardPoints,
                SuppressedChallenges = suppressedIds,
                Timestamp = dataDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "T00:00:00Z"
            });

            awardSequence++;
        }

        return results;
    }

    private static int CompareChallenges(ChallengesData left, ChallengesData right)
    {
        var priorityCompare = left.Priority.CompareTo(right.Priority);
        if (priorityCompare != 0)
        {
            return priorityCompare;
        }

        return string.Compare(left.ChallengeId, right.ChallengeId, StringComparison.OrdinalIgnoreCase);
    }

    private static string JoinChallengeIds(List<ChallengesData> challenges, int startIndex)
    {
        var ids = new List<string>();
        for (var i = startIndex; i < challenges.Count; i++)
        {
            ids.Add(challenges[i].ChallengeId);
        }

        return string.Join("|", ids);
    }

    private static bool EvaluateCondition(string condition, UserStateData userState)
    {
        if (string.IsNullOrWhiteSpace(condition) || userState == null)
        {
            return false;
        }

        var match = ConditionRegex.Match(condition.Trim());
        if (!match.Success)
        {
            return false;
        }

        var field = match.Groups[1].Value;
        var op = match.Groups[2].Value;
        var rawTarget = match.Groups[3].Value;

        if (!int.TryParse(rawTarget, NumberStyles.Integer, CultureInfo.InvariantCulture, out var target))
        {
            return false;
        }

        if (!TryGetMetricValue(userState, field, out var source))
        {
            return false;
        }

        switch (op)
        {
            case ">=": return source >= target;
            case "<=": return source <= target;
            case "==": return source == target;
            case ">": return source > target;
            case "<": return source < target;
            default: return false;
        }
    }

    private static bool TryGetMetricValue(UserStateData userState, string field, out int value)
    {
        value = 0;

        switch (field)
        {
            case "messages_today":
                value = userState.MessagesToday;
                return true;
            case "reactions_today":
                value = userState.ReactionsToday;
                return true;
            case "unique_groups_today":
                value = userState.UniqueGroupsToday;
                return true;
            case "messages_7d":
                value = userState.Messages7d;
                return true;
            case "reactions_7d":
                value = userState.Reactions7d;
                return true;
            default:
                return false;
        }
    }
}
