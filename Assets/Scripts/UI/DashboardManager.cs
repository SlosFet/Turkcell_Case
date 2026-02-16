using UnityEngine;

public class DashboardManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private LeaderboardPanel leaderboardPanel;
    [SerializeField] private UserDetailPanel userDetailPanel;

    private InputDataStore cachedInputDataStore;
    private OutputDataStore cachedOutputDataStore;

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

        if (leaderboardPanel == null)
        {
            Debug.LogError("DashboardManager: LeaderboardPanel reference is missing.");
            return;
        }

        leaderboardPanel.Render(inputDataStore, outputDataStore, OpenUserDetailFromLeaderboard);
        OpenLeaderboardPanel();
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
            userDetailPanel.ShowUser(userId, cachedInputDataStore, cachedOutputDataStore);
            userDetailPanel.Open();
        }
    }
}
