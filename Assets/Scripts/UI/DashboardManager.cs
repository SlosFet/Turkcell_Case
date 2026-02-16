using UnityEngine;

public class DashboardManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private LeaderboardPanel leaderboardPanel;
    [SerializeField] private UserDetailPanel userDetailPanel;

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

        userDetailPanel.Initialize(HandleUserDetailBackClicked);
        isUserDetailInitialized = true;
    }

    public void OpenLeaderboardPanel()
    {
        if (userDetailPanel != null)
        {
            userDetailPanel.Close();
        }

        if (leaderboardPanel != null)
        {
            leaderboardPanel.Open();
        }
    }

    private void OpenUserDetailFromLeaderboard(string userId)
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.Close();
        }

        if (userDetailPanel != null)
        {
            userDetailPanel.ShowUser(userId, cachedInputDataStore, cachedOutputDataStore, spriteMapper);
            userDetailPanel.Open();
        }
    }

    private void HandleUserDetailBackClicked()
    {
        OpenLeaderboardPanel();
    }
}
