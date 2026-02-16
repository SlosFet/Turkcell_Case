using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class DashboardManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private LeaderboardPanel leaderboardPanel;
    [SerializeField] private AddUserPanel addUserPanel;
    [SerializeField] private WhatIfPanel whatIfPanel;

    [Header("Data Stores")]
    [SerializeField] private OutputDataStore _outputDataStore;
    
    private InputDataStore inputDataStore;

    private void OnEnable()
    {
        Case3PipelineRunner.OnPipelineCompleted += HandlePipelineCompleted;
    }

    private void OnDisable()
    {
        Case3PipelineRunner.OnPipelineCompleted -= HandlePipelineCompleted;
    }

    private void HandlePipelineCompleted(InputDataStore inputDataStore, OutputDataStore outputDataStore)
    {
        this.inputDataStore = inputDataStore;
        this._outputDataStore = outputDataStore;

        if (leaderboardPanel == null)
        {
            Debug.LogError("DashboardManager: LeaderboardPanel reference is missing.");
            return;
        }

        leaderboardPanel.Initialize(OpenAddUserPanel, OpenWhatIfPanel);
        leaderboardPanel.Render(inputDataStore, outputDataStore, HandleUserSelected);
        
        addUserPanel.Initialize(AddUser);
        whatIfPanel.Initialize(RecalculateWhatIf);

        OpenLeaderboardPanel();
    }

    public void OpenAddUserPanel()
    {
        if (addUserPanel != null)
        {
            addUserPanel.Open();
        }
    }

    public void OpenWhatIfPanel()
    {
        if (whatIfPanel != null)
        {
            whatIfPanel.Render(inputDataStore, _outputDataStore);
            whatIfPanel.Open();
            leaderboardPanel.Close();
        }
    }
    
    private void AddUser(string name, string city, List<DailyActivity> dailyActivities)
    {
        var newUserId = $"U{inputDataStore.Users.Count + 1}";
        var newUser = new UsersData
        {
            UserId = newUserId,
            Name = name,
            City = city,
        };
        inputDataStore.Users.Add(newUser);

        for (int i = 0; i < dailyActivities.Count; i++)
        {
            var activity = dailyActivities[i];
            var newActivity = new ActivityEventsData
            {
                EventId = $"E{inputDataStore.ActivityEvents.Count + 1}",
                UserId = newUserId,
                Date = DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"),
                Messages = activity.Messages,
                Reactions = activity.Reactions,
                UniqueGroups = 1,
            };
            inputDataStore.ActivityEvents.Add(newActivity);
        }

        RunPipeline(inputDataStore, _outputDataStore, (newInput, newOutput) => {
            leaderboardPanel.Render(newInput, newOutput, HandleUserSelected);
        });
    }
    
    private void RecalculateWhatIf(List<WhatIfLeaderboardItem> items)
    {
        var originalValues = new Dictionary<ActivityEventsData, (int, int)>();

        foreach (var item in items)
        {
            var activity = item.GetActivity();
            originalValues[activity] = (activity.Messages, activity.Reactions);

            if (int.TryParse(item.messagesInput.text, out int messages))
            {
                activity.Messages = messages;
            }
            if (int.TryParse(item.reactionsInput.text, out int reactions))
            {
                activity.Reactions = reactions;
            }
        }
        
        var whatIfOutputStore = new OutputDataStore();
        RunPipeline(inputDataStore, whatIfOutputStore, (newInput, newOutput) => {
            whatIfPanel.Render(newInput, newOutput);
        });

        // Revert changes
        foreach (var entry in originalValues)
        {
            entry.Key.Messages = entry.Value.Item1;
            entry.Key.Reactions = entry.Value.Item2;
        }
    }

    private void RunPipeline(InputDataStore input, OutputDataStore output, Action<InputDataStore, OutputDataStore> onComplete)
    {
        Debug.Log("[DashboardManager.Pipeline] Started.");

        if (!TryResolveDataDate(input.ActivityEvents, out var dataDate))
        {
            Debug.LogError("[DashboardManager.Pipeline] No valid date found in input activity events.");
            return;
        }

        var metricCalculator = new MetricCalculator();
        var userState = metricCalculator.Calculate(input.Users, input.ActivityEvents, dataDate);
        output.UserState = userState;

        var challengeCalculator = new ChallengeCalculator();
        var challengeAwards = challengeCalculator.Calculate(input.Challenges, userState, dataDate);
        output.ChallengeAwards = challengeAwards;

        var ledgerCalculator = new LedgerCalculator();
        var pointsLedger = ledgerCalculator.Calculate(challengeAwards);
        output.PointsLedger = pointsLedger;

        var leaderboardCalculator = new LeaderboardCalculator();
        var leaderboard = leaderboardCalculator.Calculate(pointsLedger);
        output.Leaderboard = leaderboard;

        var badgeCalculator = new BadgeCalculator();
        var badgeAwards = badgeCalculator.Calculate(input.Badges, leaderboard, dataDate);
        output.BadgeAwards = badgeAwards;
        
        var notificationCalculator = new NotificationCalculator();
        var notifications = notificationCalculator.Calculate(challengeAwards);
        output.Notifications = notifications;

        onComplete?.Invoke(input, output);

        Debug.Log("[DashboardManager.Pipeline] Completed.");
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

    public void OpenLeaderboardPanel()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.Open();
            whatIfPanel.Close();
        }
    }

    private void HandleUserSelected(string userId)
    {
        Debug.Log($"[Dashboard] User selected from leaderboard: {userId}");
        // Next step: open user detail panel.
    }
}
