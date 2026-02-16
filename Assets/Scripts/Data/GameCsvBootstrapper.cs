using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameCsvBootstrapper : MonoBehaviour
{
    public static event Action<InputDataStore> OnInputDataLoaded;

    [Header("Data Stores")]
    [SerializeField] private InputDataStore inputDataStore;

    [Header("Optional Input CSV overrides")]
    [SerializeField] private TextAsset groupsCsv;
    [SerializeField] private TextAsset challengesCsv;
    [SerializeField] private TextAsset badgesCsv;

    private void Awake()
    {
        LoadStaticInputData();
    }

    private void OnEnable()
    {
        AddUserPanel.OnUserDataSubmitted += HandleUserDataSubmitted;
    }

    private void OnDisable()
    {
        AddUserPanel.OnUserDataSubmitted -= HandleUserDataSubmitted;
    }

    private void LoadStaticInputData()
    {
        if (inputDataStore == null)
        {
            Debug.LogError("GameCsvBootstrapper: InputDataStore reference is missing.");
            return;
        }

        inputDataStore.Groups = CsvMapper.MapRows<GroupsData>(LoadCsvText(groupsCsv, "groups"));
        inputDataStore.Challenges = CsvMapper.MapRows<ChallengesData>(LoadCsvText(challengesCsv, "challenges"));
        inputDataStore.Badges = CsvMapper.MapRows<BadgesData>(LoadCsvText(badgesCsv, "badges"));

        InputDataStore.SetCurrent(inputDataStore);

        Debug.Log(
            $"Static input data loaded. Groups: {inputDataStore.Groups.Count}, " +
            $"Challenges: {inputDataStore.Challenges.Count}, Badges: {inputDataStore.Badges.Count}");
    }

    private void HandleUserDataSubmitted(List<UsersData> users, List<ActivityEventsData> activityEvents)
    {
        if (inputDataStore == null)
        {
            Debug.LogError("GameCsvBootstrapper: InputDataStore reference is missing.");
            return;
        }

        inputDataStore.Users = users ?? new List<UsersData>();
        inputDataStore.ActivityEvents = activityEvents ?? new List<ActivityEventsData>();

        InputDataStore.SetCurrent(inputDataStore);
        OnInputDataLoaded?.Invoke(inputDataStore);

        Debug.Log(
            $"Runtime user data received. Users: {inputDataStore.Users.Count}, " +
            $"Events: {inputDataStore.ActivityEvents.Count}");
    }

    private string LoadCsvText(TextAsset overrideAsset, string fileNameWithoutExtension)
    {
        if (overrideAsset != null)
        {
            return overrideAsset.text;
        }

        var resource = Resources.Load<TextAsset>($"Docs/Data/{fileNameWithoutExtension}");
        if (resource != null)
        {
            return resource.text;
        }

        var absolutePath = Path.Combine(Application.dataPath, "Docs", "Data", fileNameWithoutExtension + ".csv");
        if (File.Exists(absolutePath))
        {
            return File.ReadAllText(absolutePath);
        }

        Debug.LogWarning($"CSV file not found: {fileNameWithoutExtension}.csv");
        return string.Empty;
    }
}
