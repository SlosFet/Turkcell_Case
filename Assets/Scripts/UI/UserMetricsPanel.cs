using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserMetricsPanel : UI_Panel
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform listRoot;
    [SerializeField] private UserMetricsItem itemPrefab;

    private readonly List<UserMetricsItem> spawnedItems = new List<UserMetricsItem>();
    private bool isInitialized;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        isInitialized = true;
    }

    public void ShowForUser(string userId, List<ActivityEventsData> activityEvents)
    {
        ClearItems();

        if (string.IsNullOrWhiteSpace(userId) || listRoot == null || itemPrefab == null)
        {
            return;
        }

        activityEvents = activityEvents ?? new List<ActivityEventsData>();

        var userActivities = new List<ActivityEventsData>();
        for (var i = 0; i < activityEvents.Count; i++)
        {
            var activity = activityEvents[i];
            if (activity != null && string.Equals(activity.UserId, userId, StringComparison.OrdinalIgnoreCase))
            {
                userActivities.Add(activity);
            }
        }

        userActivities.Sort((a, b) => string.Compare(a.Date, b.Date, StringComparison.Ordinal));

        for (var i = 0; i < userActivities.Count; i++)
        {
            var activity = userActivities[i];
            var row = Instantiate(itemPrefab, listRoot);
            row.Bind(
                activity.EventId,
                activity.UserId,
                activity.Date,
                activity.Messages,
                activity.Reactions,
                activity.UniqueGroups);
            spawnedItems.Add(row);
        }
    }

    private void ClearItems()
    {
        for (var i = 0; i < spawnedItems.Count; i++)
        {
            if (spawnedItems[i] != null)
            {
                Destroy(spawnedItems[i].gameObject);
            }
        }

        spawnedItems.Clear();
    }
}
