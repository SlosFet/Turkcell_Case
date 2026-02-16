using UnityEngine;

[DefaultExecutionOrder(-900)]
public sealed class Case3PipelineRunner : MonoBehaviour
{
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

        RunPipeline(inputDataStore);
    }

    private void RunPipeline(InputDataStore inputDataStore)
    {
        Debug.Log("[Pipeline] Started.");

        // Step 1 already done in bootstrapper: input CSVs are loaded into InputDataStore.
        Debug.Log("[Pipeline] Step 1 complete: Input data is ready.");

        // Step 2 will be implemented next: user metrics calculation (today / 7d).
        Debug.Log("[Pipeline] Step 2 pending: MetricCalculator will run here.");

        Debug.Log("[Pipeline] Completed.");
    }
}
