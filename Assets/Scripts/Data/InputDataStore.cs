using System.Collections.Generic;
using UnityEngine;

public sealed class InputDataStore : MonoBehaviour
{
    public static InputDataStore Current { get; private set; }

    [Header("Input CSV Data")]
    public List<UsersData> Users = new List<UsersData>();
    public List<GroupsData> Groups = new List<GroupsData>();
    public List<ActivityEventsData> ActivityEvents = new List<ActivityEventsData>();
    public List<ChallengesData> Challenges = new List<ChallengesData>();
    public List<BadgesData> Badges = new List<BadgesData>();

    public static void SetCurrent(InputDataStore store)
    {
        Current = store;
    }
}
