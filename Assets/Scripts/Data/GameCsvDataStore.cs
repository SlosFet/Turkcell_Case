using System.Collections.Generic;
using UnityEngine;

public sealed class GameCsvDataStore : MonoBehaviour
{
    public static GameCsvDataStore Current { get; private set; }

    [Header("Loaded CSV Data")]
    public List<ActivityEventsData> ActivityEvents = new List<ActivityEventsData>();
    public List<BadgesData> Badges = new List<BadgesData>();
    public List<BadgeAwardsData> BadgeAwards = new List<BadgeAwardsData>();
    public List<ChallengesData> Challenges = new List<ChallengesData>();
    public List<ChallengeAwardsData> ChallengeAwards = new List<ChallengeAwardsData>();
    public List<GroupsData> Groups = new List<GroupsData>();
    public List<LeaderboardData> Leaderboard = new List<LeaderboardData>();
    public List<NotificationsData> Notifications = new List<NotificationsData>();
    public List<PointsLedgerData> PointsLedger = new List<PointsLedgerData>();
    public List<UsersData> Users = new List<UsersData>();
    public List<UserStateData> UserState = new List<UserStateData>();

    public static void SetCurrent(GameCsvDataStore store)
    {
        Current = store;
    }
}
