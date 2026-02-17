using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Case3PipelineRunner : MonoBehaviour
{
    public static event Action<InputDataStore, OutputDataStore> OnPipelineCompleted;

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

        var orderedDataDates = GetOrderedDataDates(inputDataStore.ActivityEvents);
        if (orderedDataDates.Count == 0)
        {
            Debug.LogError("[Pipeline] No valid date found in input activity events.");
            return;
        }

        var metricCalculator = new MetricCalculator();
        var challengeCalculator = new ChallengeCalculator();

        var allChallengeAwards = new List<ChallengeAwardsData>();
        List<UserStateData> latestUserState = null;
        var nextAwardSequence = 100;

        for (var i = 0; i < orderedDataDates.Count; i++)
        {
            var dataDate = orderedDataDates[i];
            var userStateForDate = metricCalculator.Calculate(inputDataStore.Users, inputDataStore.ActivityEvents, dataDate);
            latestUserState = userStateForDate;

            var challengeAwardsForDate = challengeCalculator.Calculate(
                inputDataStore.Challenges,
                userStateForDate,
                dataDate,
                nextAwardSequence);

            nextAwardSequence += challengeAwardsForDate.Count;
            allChallengeAwards.AddRange(challengeAwardsForDate);
        }

        _outputDataStore.UserState = latestUserState ?? new List<UserStateData>();
        _outputDataStore.ChallengeAwards = allChallengeAwards;

        var latestDataDate = orderedDataDates[orderedDataDates.Count - 1];
        Debug.Log($"[Pipeline] Metrics calculated up to {latestDataDate:yyyy-MM-dd}. UserState count: {_outputDataStore.UserState.Count}");
        Debug.Log($"[Pipeline] Challenge awards calculated across {orderedDataDates.Count} day(s). Count: {allChallengeAwards.Count}");

        var ledgerCalculator = new LedgerCalculator();
        var pointsLedger = ledgerCalculator.Calculate(allChallengeAwards);
        _outputDataStore.PointsLedger = pointsLedger;

        Debug.Log($"[Pipeline] Points ledger calculated. Count: {pointsLedger.Count}");

        var leaderboardCalculator = new LeaderboardCalculator();
        var leaderboard = leaderboardCalculator.Calculate(pointsLedger);
        _outputDataStore.Leaderboard = leaderboard;

        Debug.Log($"[Pipeline] Leaderboard calculated. Count: {leaderboard.Count}");

        var badgeCalculator = new BadgeCalculator();
        var badgeAwards = badgeCalculator.Calculate(inputDataStore.Badges, leaderboard, latestDataDate);
        _outputDataStore.BadgeAwards = badgeAwards;

        Debug.Log($"[Pipeline] Badge awards calculated. Count: {badgeAwards.Count}");

        var notificationCalculator = new NotificationCalculator();
        var notifications = notificationCalculator.Calculate(allChallengeAwards);
        _outputDataStore.Notifications = notifications;

        Debug.Log($"[Pipeline] Notifications calculated. Count: {notifications.Count}");

        OutputDataStore.SetCurrent(_outputDataStore);
        OnPipelineCompleted?.Invoke(inputDataStore, _outputDataStore);

        Debug.Log("[Pipeline] Completed.");
    }

    public static bool TryResolveDataDate(List<ActivityEventsData> activityEvents, out DateTime dataDate)
    {
        dataDate = DateTime.MinValue;
        activityEvents = activityEvents ?? new List<ActivityEventsData>();

        if (activityEvents.Count == 0)
        {
            return false;
        }

        var hasLatest = false;
        var latest = DateTime.MinValue;

        for (var i = 0; i < activityEvents.Count; i++)
        {
            var evt = activityEvents[i];
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

    private static List<DateTime> GetOrderedDataDates(List<ActivityEventsData> activityEvents)
    {
        var dates = new HashSet<DateTime>();
        var result = new List<DateTime>();
        activityEvents = activityEvents ?? new List<ActivityEventsData>();

        for (var i = 0; i < activityEvents.Count; i++)
        {
            var evt = activityEvents[i];
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

            dates.Add(parsedDate.Date);
        }

        foreach (var date in dates)
        {
            result.Add(date);
        }

        result.Sort();
        return result;
    }
}
