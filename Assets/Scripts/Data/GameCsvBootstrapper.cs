using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public sealed class GameCsvBootstrapper : MonoBehaviour
{
    [Header("Optional CSV overrides")]
    [SerializeField] private TextAsset activityEventsCsv;
    [SerializeField] private TextAsset badgesCsv;
    [SerializeField] private TextAsset badgeAwardsCsv;
    [SerializeField] private TextAsset challengesCsv;
    [SerializeField] private TextAsset challengeAwardsCsv;
    [SerializeField] private TextAsset groupsCsv;
    [SerializeField] private TextAsset leaderboardCsv;
    [SerializeField] private TextAsset notificationsCsv;
    [SerializeField] private TextAsset pointsLedgerCsv;
    [SerializeField] private TextAsset usersCsv;
    [SerializeField] private TextAsset userStateCsv;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        var store = GetComponent<GameCsvDataStore>();
        if (store == null)
        {
            store = gameObject.AddComponent<GameCsvDataStore>();
        }

        store.ActivityEvents = CsvMapper.MapRows<ActivityEventsData>(LoadCsvText(activityEventsCsv, "activity_events"));
        store.Badges = CsvMapper.MapRows<BadgesData>(LoadCsvText(badgesCsv, "badges"));
        store.BadgeAwards = CsvMapper.MapRows<BadgeAwardsData>(LoadCsvText(badgeAwardsCsv, "badge_awards"));
        store.Challenges = CsvMapper.MapRows<ChallengesData>(LoadCsvText(challengesCsv, "challenges"));
        store.ChallengeAwards = CsvMapper.MapRows<ChallengeAwardsData>(LoadCsvText(challengeAwardsCsv, "challenge_awards"));
        store.Groups = CsvMapper.MapRows<GroupsData>(LoadCsvText(groupsCsv, "groups"));
        store.Leaderboard = CsvMapper.MapRows<LeaderboardData>(LoadCsvText(leaderboardCsv, "leaderboard"));
        store.Notifications = CsvMapper.MapRows<NotificationsData>(LoadCsvText(notificationsCsv, "notifications"));
        store.PointsLedger = CsvMapper.MapRows<PointsLedgerData>(LoadCsvText(pointsLedgerCsv, "points_ledger"));
        store.Users = CsvMapper.MapRows<UsersData>(LoadCsvText(usersCsv, "users"));
        store.UserState = CsvMapper.MapRows<UserStateData>(LoadCsvText(userStateCsv, "user_state"));

        GameCsvDataStore.SetCurrent(store);
        Debug.Log($"CSV data loaded. Users: {store.Users.Count}, Challenges: {store.Challenges.Count}");
    }

    private static string LoadCsvText(TextAsset overrideAsset, string fileNameWithoutExtension)
    {
        if (overrideAsset != null)
        {
            return overrideAsset.text;
        }

        var resource = Resources.Load<TextAsset>($"Docs/Data/{fileNameWithoutExtension}");
        if (resource != null)
        {
            return resource.text;
        }

        var absolutePath = Path.Combine(Application.dataPath, "Docs", "Data", fileNameWithoutExtension + ".csv");
        if (File.Exists(absolutePath))
        {
            return File.ReadAllText(absolutePath);
        }

        Debug.LogWarning($"CSV file not found: {fileNameWithoutExtension}.csv");
        return string.Empty;
    }
}
