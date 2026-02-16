using UnityEngine;
using UnityEngine.UI;

public class WhatIfLeaderboardItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] public Text rankText;
    [SerializeField] public Text nameText;
    [SerializeField] public InputField messagesInput;
    [SerializeField] public InputField reactionsInput;
    [SerializeField] public Text totalPointsText;

    private UsersData user;
    private ActivityEventsData activity;

    public void Bind(UsersData user, ActivityEventsData activity, LeaderboardData leaderboardData)
    {
        this.user = user;
        this.activity = activity;
        rankText.text = leaderboardData.Rank.ToString();
        nameText.text = user.Name;
        messagesInput.text = activity.Messages.ToString();
        reactionsInput.text = activity.Reactions.ToString();
        totalPointsText.text = leaderboardData.TotalPoints.ToString();
    }

    public UsersData GetUser()
    {
        return user;
    }

    public ActivityEventsData GetActivity()
    {
        return activity;
    }
}
