using System;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardPanel : UI_Panel
{
    [Serializable]
    private class BadgeSpriteEntry
    {
        public string badgeId;
        public Sprite sprite;
    }

    [Header("List")]
    [SerializeField] private Transform listRoot;
    [SerializeField] private LeaderboardItem itemPrefab;

    [Header("Visual Defaults")]
    [SerializeField] private Sprite defaultUserPhoto;
    [SerializeField] private Sprite defaultBadgeSprite;
    [SerializeField] private List<BadgeSpriteEntry> badgeSprites = new List<BadgeSpriteEntry>();

    private readonly List<LeaderboardItem> spawnedItems = new List<LeaderboardItem>();

    public void Render(InputDataStore inputDataStore, OutputDataStore outputDataStore, Action<string> onUserSelected)
    {
        ClearItems();

        if (inputDataStore == null || outputDataStore == null || listRoot == null || itemPrefab == null)
        {
            return;
        }

        var usersById = BuildUsersMap(inputDataStore.Users);
        var badgeByUser = BuildHighestBadgeByUser(outputDataStore.BadgeAwards);

        var leaderboard = outputDataStore.Leaderboard ?? new List<LeaderboardData>();
        for (var i = 0; i < leaderboard.Count; i++)
        {
            var row = leaderboard[i];
            if (row == null || string.IsNullOrWhiteSpace(row.UserId))
            {
                continue;
            }

            var item = Instantiate(itemPrefab, listRoot);
            spawnedItems.Add(item);

            usersById.TryGetValue(row.UserId, out var user);
            badgeByUser.TryGetValue(row.UserId, out var badgeId);

            var fullName = user != null ? user.Name : row.UserId;
            var badgeSprite = ResolveBadgeSprite(badgeId);

            item.Bind(
                row.UserId,
                fullName,
                row.Rank,
                row.TotalPoints,
                defaultUserPhoto,
                badgeSprite,
                onUserSelected);
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

    private Dictionary<string, UsersData> BuildUsersMap(List<UsersData> users)
    {
        var map = new Dictionary<string, UsersData>(StringComparer.OrdinalIgnoreCase);
        users = users ?? new List<UsersData>();

        for (var i = 0; i < users.Count; i++)
        {
            var user = users[i];
            if (user == null || string.IsNullOrWhiteSpace(user.UserId) || map.ContainsKey(user.UserId))
            {
                continue;
            }

            map[user.UserId] = user;
        }

        return map;
    }

    private Dictionary<string, string> BuildHighestBadgeByUser(List<BadgeAwardsData> badgeAwards)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var levels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        badgeAwards = badgeAwards ?? new List<BadgeAwardsData>();

        for (var i = 0; i < badgeAwards.Count; i++)
        {
            var award = badgeAwards[i];
            if (award == null || string.IsNullOrWhiteSpace(award.UserId) || string.IsNullOrWhiteSpace(award.BadgeId))
            {
                continue;
            }

            var level = ParseBadgeLevel(award.BadgeId);
            if (!levels.ContainsKey(award.UserId) || level > levels[award.UserId])
            {
                levels[award.UserId] = level;
                map[award.UserId] = award.BadgeId;
            }
        }

        return map;
    }

    private Sprite ResolveBadgeSprite(string badgeId)
    {
        if (string.IsNullOrWhiteSpace(badgeId))
        {
            return defaultBadgeSprite;
        }

        for (var i = 0; i < badgeSprites.Count; i++)
        {
            var entry = badgeSprites[i];
            if (entry != null && string.Equals(entry.badgeId, badgeId, StringComparison.OrdinalIgnoreCase))
            {
                return entry.sprite != null ? entry.sprite : defaultBadgeSprite;
            }
        }

        return defaultBadgeSprite;
    }

    private int ParseBadgeLevel(string badgeId)
    {
        // B1, B2, B3 pattern for quick level comparison
        if (string.IsNullOrWhiteSpace(badgeId) || badgeId.Length < 2)
        {
            return 0;
        }

        var numeric = badgeId.Substring(1);
        return int.TryParse(numeric, out var level) ? level : 0;
    }
}
