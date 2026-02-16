using System;

[Serializable]
public class ChallengeAwardsData
{
    public string AwardId;
    public string UserId;
    public string TriggeredChallenges;
    public string SelectedChallenge;
    public int RewardPoints;
    public string SuppressedChallenges;
    public string Timestamp;
}
