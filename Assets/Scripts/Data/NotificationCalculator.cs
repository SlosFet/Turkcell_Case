using System.Collections.Generic;
using System.Globalization;

public class NotificationCalculator
{
    public List<NotificationsData> Calculate(List<ChallengeAwardsData> challengeAwards)
    {
        var result = new List<NotificationsData>();
        if (challengeAwards == null)
        {
            return result;
        }

        var notificationSequence = 300;
        for (var i = 0; i < challengeAwards.Count; i++)
        {
            var award = challengeAwards[i];
            if (award == null || string.IsNullOrWhiteSpace(award.UserId) || string.IsNullOrWhiteSpace(award.SelectedChallenge))
            {
                continue;
            }

            result.Add(new NotificationsData
            {
                NotificationId = "N-" + notificationSequence.ToString(CultureInfo.InvariantCulture),
                UserId = award.UserId,
                Message = award.SelectedChallenge + " görevi tamamlandı. +" + award.RewardPoints.ToString(CultureInfo.InvariantCulture) + " puan.",
                SentAt = award.Timestamp
            });

            notificationSequence++;
        }

        return result;
    }
}
