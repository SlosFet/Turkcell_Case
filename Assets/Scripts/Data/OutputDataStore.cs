using System.Collections.Generic;
using UnityEngine;

public sealed class OutputDataStore : MonoBehaviour
{
    public static OutputDataStore Current { get; private set; }

    [Header("Generated Output Data")]
    public List<UserStateData> UserState = new List<UserStateData>();
    public List<ChallengeAwardsData> ChallengeAwards = new List<ChallengeAwardsData>();
    public List<PointsLedgerData> PointsLedger = new List<PointsLedgerData>();
    public List<LeaderboardData> Leaderboard = new List<LeaderboardData>();
    public List<BadgeAwardsData> BadgeAwards = new List<BadgeAwardsData>();
    public List<NotificationsData> Notifications = new List<NotificationsData>();

    public static void SetCurrent(OutputDataStore store)
    {
        Current = store;
    }
}
