using UnityEngine;

public class DashboardManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private LeaderboardPanel leaderboardPanel;

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
        if (leaderboardPanel == null)
        {
            Debug.LogError("DashboardManager: LeaderboardPanel reference is missing.");
            return;
        }

        leaderboardPanel.Render(inputDataStore, outputDataStore, HandleUserSelected);
        OpenLeaderboardPanel();
    }

    public void OpenLeaderboardPanel()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.Open();
        }
    }

    private void HandleUserSelected(string userId)
    {
        Debug.Log($"[Dashboard] User selected from leaderboard: {userId}");
        // Next step: open user detail panel.
    }
}
