using System;
using System.Collections.Generic;
using UnityEngine;

public class UserBadgeSpriteMapper : MonoBehaviour
{
    [Serializable]
    private class UserSpriteEntry
    {
        public string userId;
        public Sprite sprite;
    }

    [Serializable]
    private class BadgeSpriteEntry
    {
        public string badgeId;
        public Sprite sprite;
    }

    [Header("Defaults")]
    [SerializeField] private Sprite defaultUserSprite;
    [SerializeField] private Sprite defaultBadgeSprite;

    [Header("Mappings")]
    [SerializeField] private List<UserSpriteEntry> userSprites = new List<UserSpriteEntry>();
    [SerializeField] private List<BadgeSpriteEntry> badgeSprites = new List<BadgeSpriteEntry>();

    public Sprite GetUserSprite(string userId)
    {
        if (!string.IsNullOrWhiteSpace(userId))
        {
            for (var i = 0; i < userSprites.Count; i++)
            {
                var entry = userSprites[i];
                if (entry != null && string.Equals(entry.userId, userId, StringComparison.OrdinalIgnoreCase))
                {
                    return entry.sprite != null ? entry.sprite : defaultUserSprite;
                }
            }
        }

        return defaultUserSprite;
    }

    public Sprite GetBadgeSprite(string badgeId)
    {
        if (!string.IsNullOrWhiteSpace(badgeId))
        {
            for (var i = 0; i < badgeSprites.Count; i++)
            {
                var entry = badgeSprites[i];
                if (entry != null && string.Equals(entry.badgeId, badgeId, StringComparison.OrdinalIgnoreCase))
                {
                    return entry.sprite != null ? entry.sprite : defaultBadgeSprite;
                }
            }
        }

        return defaultBadgeSprite;
    }
}
