using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationPanel : UI_Panel
{
    [Header("List")]
    [SerializeField] private Transform listRoot;
    [SerializeField] private NotificationItem itemPrefab;

    [Header("Actions")]
    [SerializeField] private Button closeButton;

    private readonly List<NotificationItem> spawnedItems = new List<NotificationItem>();
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

    public void ShowForUser(string userId, OutputDataStore outputDataStore)
    {
        ClearItems();

        if (string.IsNullOrWhiteSpace(userId) || outputDataStore == null || listRoot == null || itemPrefab == null)
        {
            return;
        }

        var notifications = outputDataStore.Notifications ?? new List<NotificationsData>();
        for (var i = 0; i < notifications.Count; i++)
        {
            var item = notifications[i];
            if (item == null || !string.Equals(item.UserId, userId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var row = Instantiate(itemPrefab, listRoot);
            row.Bind(item.Message);
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
