using UnityEngine;

public class Case3PipelineRunner : MonoBehaviour
{
    [Header("Data Stores")]
    [SerializeField] private OutputDataStore _outputDataStore;

    [Header("Metrics")]
    [SerializeField] private string asOfDate = string.Empty; // yyyy-MM-dd (empty => latest event date)

    [SerializeField] private MetricCalculator _metricCalculator;

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
        var resolvedAsOfDate = metricCalculator.ResolveAsOfDate(inputDataStore, asOfDate);
        var userState = metricCalculator.Calculate(inputDataStore, resolvedAsOfDate);

        _outputDataStore.UserState = userState;
        OutputDataStore.SetCurrent(_outputDataStore);

        Debug.Log($"[Pipeline] Metrics calculated for {resolvedAsOfDate:yyyy-MM-dd}. UserState count: {userState.Count}");
        Debug.Log("[Pipeline] Completed.");
    }
}
