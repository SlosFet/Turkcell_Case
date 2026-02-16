using UnityEngine;

public class Case3PipelineRunner : MonoBehaviour
{
    [Header("Data Stores")]
    [SerializeField] private OutputDataStore _outputDataStore;

    private void OnEnable()
    {
        GameCsvBootstrapper.OnInputDataLoaded += HandleInputDataLoaded;
    }

    private void OnDisable()
    {
        GameCsvBootstrapper.OnInputDataLoaded -= HandleInputDataLoaded;
    }

    private void HandleInputDataLoaded(InputDataStore inputDataStore)
    {
        if (inputDataStore == null)
        {
            Debug.LogError("Case3PipelineRunner: InputDataStore is null.");
            return;
        }

        if (_outputDataStore == null)
        {
            Debug.LogError("Case3PipelineRunner: OutputDataStore reference is missing.");
            return;
        }

        RunPipeline(inputDataStore);
    }

    private void RunPipeline(InputDataStore inputDataStore)
    {
        Debug.Log("[Pipeline] Started.");

        var metricCalculator = new MetricCalculator();
        if (!metricCalculator.TryResolveAsOfDate(inputDataStore, out var resolvedAsOfDate))
        {
            Debug.LogError("[Pipeline] No valid date found in input activity events.");
            return;
        }

        var userState = metricCalculator.Calculate(inputDataStore);
        _outputDataStore.UserState = userState;

        Debug.Log($"[Pipeline] Metrics calculated for data date {resolvedAsOfDate:yyyy-MM-dd}. UserState count: {userState.Count}");

        var challengeCalculator = new ChallengeCalculator();
        var challengeAwards = challengeCalculator.Calculate(inputDataStore, userState);
        _outputDataStore.ChallengeAwards = challengeAwards;

        Debug.Log($"[Pipeline] Challenge awards calculated for data date {resolvedAsOfDate:yyyy-MM-dd}. Count: {challengeAwards.Count}");

        OutputDataStore.SetCurrent(_outputDataStore);
        Debug.Log("[Pipeline] Completed.");
    }
}
