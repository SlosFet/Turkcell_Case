using System;
using System.Globalization;
using UnityEngine;

public class Case3PipelineRunner : MonoBehaviour
{
    [Header("Data Stores")]
    [SerializeField] private OutputDataStore _outputDataStore;

    private void OnEnable()
    {
        GameCsvBootstrapper.OnInputDataLoaded += HandleInputDataLoaded;
    }

    private void OnDisable()
    {
        GameCsvBootstrapper.OnInputDataLoaded -= HandleInputDataLoaded;
    }

    private void HandleInputDataLoaded(InputDataStore inputDataStore)
    {
        if (inputDataStore == null)
        {
            Debug.LogError("Case3PipelineRunner: InputDataStore is null.");
            return;
        }

        if (_outputDataStore == null)
        {
            Debug.LogError("Case3PipelineRunner: OutputDataStore reference is missing.");
            return;
        }

        RunPipeline(inputDataStore);
    }

    private void RunPipeline(InputDataStore inputDataStore)
    {
        Debug.Log("[Pipeline] Started.");

        if (!TryResolveDataDate(inputDataStore, out var dataDate))
        {
            Debug.LogError("[Pipeline] No valid date found in input activity events.");
            return;
        }

        var metricCalculator = new MetricCalculator();
        var userState = metricCalculator.Calculate(inputDataStore, dataDate);
        _outputDataStore.UserState = userState;

        Debug.Log($"[Pipeline] Metrics calculated for data date {dataDate:yyyy-MM-dd}. UserState count: {userState.Count}");

        var challengeCalculator = new ChallengeCalculator();
        var challengeAwards = challengeCalculator.Calculate(inputDataStore, userState, dataDate);
        _outputDataStore.ChallengeAwards = challengeAwards;

        Debug.Log($"[Pipeline] Challenge awards calculated for data date {dataDate:yyyy-MM-dd}. Count: {challengeAwards.Count}");

        var ledgerCalculator = new LedgerCalculator();
        var pointsLedger = ledgerCalculator.Calculate(challengeAwards);
        _outputDataStore.PointsLedger = pointsLedger;

        Debug.Log($"[Pipeline] Points ledger calculated. Count: {pointsLedger.Count}");

        var leaderboardCalculator = new LeaderboardCalculator();
        var leaderboard = leaderboardCalculator.Calculate(pointsLedger);
        _outputDataStore.Leaderboard = leaderboard;

        Debug.Log($"[Pipeline] Leaderboard calculated. Count: {leaderboard.Count}");

        var badgeCalculator = new BadgeCalculator();
        var badgeAwards = badgeCalculator.Calculate(inputDataStore, leaderboard, dataDate);
        _outputDataStore.BadgeAwards = badgeAwards;

        Debug.Log($"[Pipeline] Badge awards calculated. Count: {badgeAwards.Count}");

        var notificationCalculator = new NotificationCalculator();
        var notifications = notificationCalculator.Calculate(challengeAwards);
        _outputDataStore.Notifications = notifications;

        Debug.Log($"[Pipeline] Notifications calculated. Count: {notifications.Count}");

        OutputDataStore.SetCurrent(_outputDataStore);
        Debug.Log("[Pipeline] Completed.");
    }

    public static bool TryResolveDataDate(InputDataStore inputDataStore, out DateTime dataDate)
    {
        dataDate = DateTime.MinValue;
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
            if (evt == null)
            {
                continue;
            }

            if (!DateTime.TryParseExact(evt.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                if (!DateTime.TryParse(evt.Date, out parsedDate))
                {
                    continue;
                }
            }

            var current = parsedDate.Date;
            if (!hasLatest || current > latest)
            {
                latest = current;
                hasLatest = true;
            }
        }

        if (!hasLatest)
        {
            return false;
        }

        dataDate = latest;
        return true;
    }
}
