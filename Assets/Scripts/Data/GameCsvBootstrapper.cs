using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public sealed class GameCsvBootstrapper : MonoBehaviour
{
    [Header("Data Stores")]
    [SerializeField] private InputDataStore inputDataStore;

    [Header("Optional Input CSV overrides")]
    [SerializeField] private TextAsset usersCsv;
    [SerializeField] private TextAsset groupsCsv;
    [SerializeField] private TextAsset activityEventsCsv;
    [SerializeField] private TextAsset challengesCsv;
    [SerializeField] private TextAsset badgesCsv;

    private void Awake()
    {
        if (inputDataStore == null)
        {
            Debug.LogError("GameCsvBootstrapper: InputDataStore reference is missing.");
            return;
        }

        inputDataStore.Users = CsvMapper.MapRows<UsersData>(LoadCsvText(usersCsv, "users"));
        inputDataStore.Groups = CsvMapper.MapRows<GroupsData>(LoadCsvText(groupsCsv, "groups"));
        inputDataStore.ActivityEvents = CsvMapper.MapRows<ActivityEventsData>(LoadCsvText(activityEventsCsv, "activity_events"));
        inputDataStore.Challenges = CsvMapper.MapRows<ChallengesData>(LoadCsvText(challengesCsv, "challenges"));
        inputDataStore.Badges = CsvMapper.MapRows<BadgesData>(LoadCsvText(badgesCsv, "badges"));

        InputDataStore.SetCurrent(inputDataStore);

        Debug.Log(
            $"Input data loaded. Users: {inputDataStore.Users.Count}, Groups: {inputDataStore.Groups.Count}, " +
            $"Events: {inputDataStore.ActivityEvents.Count}, Challenges: {inputDataStore.Challenges.Count}, Badges: {inputDataStore.Badges.Count}");
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
