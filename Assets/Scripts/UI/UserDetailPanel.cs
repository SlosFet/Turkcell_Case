using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UserDetailPanel : UI_Panel
{
    [Header("Visual")]
    [SerializeField] private Image userPhotoImage;
    [SerializeField] private Image badgeImage;

    [Header("Texts")]
    [SerializeField] private TMP_Text fullNameText;
    [SerializeField] private TMP_Text totalPointsText;
    [SerializeField] private TMP_Text triggeredChallengesText;
    [SerializeField] private TMP_Text selectedChallengeText;

    [Header("Actions")]
    [SerializeField] private Button backToLeaderboardButton;
    [SerializeField] private Button openNotificationsButton;
    [SerializeField] private Button openUserMetricsButton;

    private bool isInitialized;

    public string CurrentUserId { get; private set; }

    public void Initialize(
        UnityAction onBackToLeaderboard,
        UnityAction<string> onOpenNotifications,
        UnityAction<string> onOpenUserMetrics)
    {
        if (isInitialized)
        {
            return;
        }

        if (backToLeaderboardButton != null)
        {
            backToLeaderboardButton.onClick.RemoveAllListeners();
            if (onBackToLeaderboard != null)
            {
                backToLeaderboardButton.onClick.AddListener(onBackToLeaderboard);
            }
        }

        if (openNotificationsButton != null)
        {
            openNotificationsButton.onClick.RemoveAllListeners();
            openNotificationsButton.onClick.AddListener(() =>
            {
                if (!string.IsNullOrWhiteSpace(CurrentUserId))
                {
                    onOpenNotifications?.Invoke(CurrentUserId);
                }
            });
        }

        if (openUserMetricsButton != null)
        {
            openUserMetricsButton.onClick.RemoveAllListeners();
            openUserMetricsButton.onClick.AddListener(() =>
            {
                if (!string.IsNullOrWhiteSpace(CurrentUserId))
                {
                    onOpenUserMetrics?.Invoke(CurrentUserId);
                }
            });
        }

        isInitialized = true;
    }

    public void ShowUser(
        string userId,
        InputDataStore inputDataStore,
        OutputDataStore outputDataStore,
        UserBadgeSpriteMapper spriteMapper)
    {
        CurrentUserId = userId;
        Render(inputDataStore, outputDataStore, spriteMapper);
    }

    private void Render(InputDataStore inputDataStore, OutputDataStore outputDataStore, UserBadgeSpriteMapper spriteMapper)
    {
        if (string.IsNullOrWhiteSpace(CurrentUserId) || inputDataStore == null || outputDataStore == null)
        {
            return;
        }

        var user = FindUser(inputDataStore.Users, CurrentUserId);
        var leaderboardRow = FindLeaderboardRow(outputDataStore.Leaderboard, CurrentUserId);
        var userAwards = FindChallengeAwards(outputDataStore.ChallengeAwards, CurrentUserId);
        var highestBadgeId = FindHighestBadgeId(outputDataStore.BadgeAwards, CurrentUserId);
        var hasNotifications = HasNotifications(outputDataStore.Notifications, CurrentUserId);
        var hasActivities = HasActivities(inputDataStore.ActivityEvents, CurrentUserId);

        SetText(fullNameText, user != null && !string.IsNullOrWhiteSpace(user.Name) ? user.Name : CurrentUserId);
        SetText(totalPointsText, leaderboardRow != null ? "Total Point: " + leaderboardRow.TotalPoints.ToString() : "0");

        SetText(triggeredChallengesText, "Triggered Challenges:\n" + BuildTriggeredChallengesText(userAwards));
        SetText(selectedChallengeText, "Selected Challenge:\n" + BuildSelectedChallengeText(userAwards));

        if (openNotificationsButton != null)
        {
            openNotificationsButton.gameObject.SetActive(hasNotifications);
        }

        if (openUserMetricsButton != null)
        {
            openUserMetricsButton.gameObject.SetActive(hasActivities);
        }

        if (userPhotoImage != null)
        {
            var userSprite = spriteMapper != null ? spriteMapper.GetUserSprite(CurrentUserId) : null;
            userPhotoImage.sprite = userSprite;
            userPhotoImage.enabled = userSprite != null;
        }

        if (badgeImage != null)
        {
            var badgeSprite = spriteMapper != null ? spriteMapper.GetBadgeSprite(highestBadgeId) : null;
            badgeImage.sprite = badgeSprite;
            badgeImage.transform.parent.gameObject.SetActive(badgeSprite != null);
        }
    }

    private static UsersData FindUser(List<UsersData> users, string userId)
    {
        users = users ?? new List<UsersData>();
        for (var i = 0; i < users.Count; i++)
        {
            var user = users[i];
            if (user != null && string.Equals(user.UserId, userId, StringComparison.OrdinalIgnoreCase))
            {
                return user;
            }
        }

        return null;
    }

    private static LeaderboardData FindLeaderboardRow(List<LeaderboardData> leaderboard, string userId)
    {
        leaderboard = leaderboard ?? new List<LeaderboardData>();
        for (var i = 0; i < leaderboard.Count; i++)
        {
            var item = leaderboard[i];
            if (item != null && string.Equals(item.UserId, userId, StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }
        }

        return null;
    }

    private static List<ChallengeAwardsData> FindChallengeAwards(List<ChallengeAwardsData> awards, string userId)
    {
        var result = new List<ChallengeAwardsData>();
        awards = awards ?? new List<ChallengeAwardsData>();

        for (var i = 0; i < awards.Count; i++)
        {
            var item = awards[i];
            if (item != null && string.Equals(item.UserId, userId, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(item);
            }
        }

        return result;
    }

    private static string FindHighestBadgeId(List<BadgeAwardsData> badgeAwards, string userId)
    {
        var bestBadgeId = string.Empty;
        var bestLevel = -1;
        badgeAwards = badgeAwards ?? new List<BadgeAwardsData>();

        for (var i = 0; i < badgeAwards.Count; i++)
        {
            var award = badgeAwards[i];
            if (award == null || !string.Equals(award.UserId, userId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var level = ParseBadgeLevel(award.BadgeId);
            if (level > bestLevel)
            {
                bestLevel = level;
                bestBadgeId = award.BadgeId;
            }
        }

        return bestBadgeId;
    }

    private string BuildTriggeredChallengesText(List<ChallengeAwardsData> userAwards)
    {
        if (userAwards == null || userAwards.Count == 0)
        {
            return "-";
        }

        var sb = new StringBuilder();
        for (var i = 0; i < userAwards.Count; i++)
        {
            var value = userAwards[i] != null ? userAwards[i].TriggeredChallenges : string.Empty;
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (sb.Length > 0)
            {
                sb.AppendLine();
            }

            sb.Append(value);
        }

        return sb.Length > 0 ? sb.ToString() : "-";
    }

    private string BuildSelectedChallengeText(List<ChallengeAwardsData> userAwards)
    {
        if (userAwards == null || userAwards.Count == 0)
        {
            return "-";
        }

        var sb = new StringBuilder();
        for (var i = 0; i < userAwards.Count; i++)
        {
            var value = userAwards[i] != null ? userAwards[i].SelectedChallenge : string.Empty;
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (sb.Length > 0)
            {
                sb.AppendLine();
            }

            sb.Append(value);
        }

        return sb.Length > 0 ? sb.ToString() : "-";
    }

    private static int ParseBadgeLevel(string badgeId)
    {
        if (string.IsNullOrWhiteSpace(badgeId) || badgeId.Length < 2)
        {
            return 0;
        }

        var numeric = badgeId.Substring(1);
        return int.TryParse(numeric, out var level) ? level : 0;
    }

    private static bool HasNotifications(List<NotificationsData> notifications, string userId)
    {
        notifications = notifications ?? new List<NotificationsData>();
        for (var i = 0; i < notifications.Count; i++)
        {
            var item = notifications[i];
            if (item != null && string.Equals(item.UserId, userId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasActivities(List<ActivityEventsData> activities, string userId)
    {
        activities = activities ?? new List<ActivityEventsData>();
        for (var i = 0; i < activities.Count; i++)
        {
            var item = activities[i];
            if (item != null && string.Equals(item.UserId, userId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void SetText(TMP_Text textComponent, string value)
    {
        if (textComponent != null)
        {
            textComponent.text = value;
        }
    }
}
