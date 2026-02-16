using TMPro;
using UnityEngine;

public class DashboardManager : MonoBehaviour
{
    private const string LeaderboardHeader = "Leaderboard";
    private const string ProfileHeader = "Profile Data";

    [Header("Header")]
    [SerializeField] private TMP_Text headerText;

    [Header("Panels")]
    [SerializeField] private LeaderboardPanel leaderboardPanel;
    [SerializeField] private UserDetailPanel userDetailPanel;
    [SerializeField] private NotificationPanel notificationPanel;
    [SerializeField] private UserMetricsPanel userMetricsPanel;

    [Header("Mappers")]
    [SerializeField] private UserBadgeSpriteMapper spriteMapper;

    private InputDataStore cachedInputDataStore;
    private OutputDataStore cachedOutputDataStore;
    private bool isUserDetailInitialized;

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
        cachedInputDataStore = inputDataStore;
        cachedOutputDataStore = outputDataStore;

        InitializeUserDetailPanel();

        if (leaderboardPanel == null)
        {
            Debug.LogError("DashboardManager: LeaderboardPanel reference is missing.");
            return;
        }

        leaderboardPanel.Render(inputDataStore, outputDataStore, spriteMapper, OpenUserDetailFromLeaderboard);
        OpenLeaderboardPanel();
    }

    private void InitializeUserDetailPanel()
    {
        if (isUserDetailInitialized)
        {
            return;
        }

        if (userDetailPanel == null)
        {
            Debug.LogError("DashboardManager: UserDetailPanel reference is missing.");
            return;
        }

        userDetailPanel.Initialize(HandleUserDetailBackClicked, OpenNotificationsFromUserDetail, OpenUserMetricsFromUserDetail);
        isUserDetailInitialized = true;
    }

    public void OpenLeaderboardPanel()
    {
        if (notificationPanel != null)
        {
            notificationPanel.Close();
        }

        if (userMetricsPanel != null)
        {
            userMetricsPanel.Close();
        }

        if (userDetailPanel != null)
        {
            userDetailPanel.Close();
        }

        if (leaderboardPanel != null)
        {
            SetHeader(LeaderboardHeader);
            leaderboardPanel.Open();
        }
    }

    private void OpenUserDetailFromLeaderboard(string userId)
    {
        if (notificationPanel != null)
        {
            notificationPanel.Close();
        }

        if (userMetricsPanel != null)
        {
            userMetricsPanel.Close();
        }

        if (leaderboardPanel != null)
        {
            leaderboardPanel.Close();
        }

        if (userDetailPanel != null)
        {
            SetHeader(ProfileHeader);
            userDetailPanel.ShowUser(userId, cachedInputDataStore, cachedOutputDataStore, spriteMapper);
            userDetailPanel.Open();
        }
    }

    private void OpenNotificationsFromUserDetail(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        if (notificationPanel != null)
        {
            notificationPanel.ShowForUser(userId, cachedOutputDataStore);
            notificationPanel.Open();
        }
    }

    private void OpenUserMetricsFromUserDetail(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        if (userMetricsPanel != null)
        {
            userMetricsPanel.ShowForUser(userId, cachedInputDataStore != null ? cachedInputDataStore.ActivityEvents : null);
            userMetricsPanel.Open();
        }
    }

    private void HandleUserDetailBackClicked()
    {
        OpenLeaderboardPanel();
    }

    private void SetHeader(string text)
    {
        if (headerText != null)
        {
            headerText.text = text;
        }
    }
}
