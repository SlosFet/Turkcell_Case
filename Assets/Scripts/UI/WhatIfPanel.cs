using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WhatIfPanel : UI_Panel
{
    [Header("UI References")]
    [SerializeField] private Transform listRoot;
    [SerializeField] private WhatIfLeaderboardItem itemPrefab;
    [SerializeField] private Button recalculateButton;
    [SerializeField] private Button closeButton;

    private readonly List<WhatIfLeaderboardItem> spawnedItems = new List<WhatIfLeaderboardItem>();
    private Action<List<WhatIfLeaderboardItem>> onRecalculateCallback;
    
    public void Initialize(Action<List<WhatIfLeaderboardItem>> onRecalculate)
    {
        this.onRecalculateCallback = onRecalculate;
    }

    private void Start()
    {
        if (recalculateButton != null)
        {
            recalculateButton.onClick.AddListener(OnRecalculateButtonClicked);
        }
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }
    }

    private void OnDestroy()
    {
        if (recalculateButton != null)
        {
            recalculateButton.onClick.RemoveListener(OnRecalculateButtonClicked);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
        }
    }

    private void OnRecalculateButtonClicked()
    {
        onRecalculateCallback?.Invoke(spawnedItems);
    }

    public void Render(InputDataStore inputDataStore, OutputDataStore outputDataStore)
    {
        ClearItems();

        if (inputDataStore == null || outputDataStore == null || listRoot == null || itemPrefab == null)
        {
            return;
        }

        var usersById = new Dictionary<string, UsersData>();
        foreach(var user in inputDataStore.Users)
        {
            usersById[user.UserId] = user;
        }

        var activityByUser = new Dictionary<string, ActivityEventsData>();
        if(inputDataStore.ActivityEvents.Count > 0)
        {
            var latestDate = inputDataStore.ActivityEvents.Max(a => a.Date);
            foreach(var activity in inputDataStore.ActivityEvents.Where(a => a.Date == latestDate))
            {
                activityByUser[activity.UserId] = activity;
            }
        }

        foreach (var leaderboardData in outputDataStore.Leaderboard)
        {
            if (usersById.TryGetValue(leaderboardData.UserId, out var user) && activityByUser.TryGetValue(leaderboardData.UserId, out var activity))
            {
                var item = Instantiate(itemPrefab, listRoot);
                item.Bind(user, activity, leaderboardData);
                spawnedItems.Add(item);
            }
        }
    }

    private void ClearItems()
    {
        foreach (var item in spawnedItems)
        {
            Destroy(item.gameObject);
        }
        spawnedItems.Clear();
    }
}
