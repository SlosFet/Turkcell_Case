using System.Collections.Generic;
using System.Globalization;

public class LedgerCalculator
{
    public List<PointsLedgerData> Calculate(List<ChallengeAwardsData> challengeAwards)
    {
        var result = new List<PointsLedgerData>();
        if (challengeAwards == null)
        {
            return result;
        }

        var ledgerSequence = 200;
        for (var i = 0; i < challengeAwards.Count; i++)
        {
            var award = challengeAwards[i];
            if (award == null || string.IsNullOrWhiteSpace(award.UserId) || string.IsNullOrWhiteSpace(award.AwardId))
            {
                continue;
            }

            result.Add(new PointsLedgerData
            {
                LedgerId = "L-" + ledgerSequence.ToString(CultureInfo.InvariantCulture),
                UserId = award.UserId,
                PointsDelta = award.RewardPoints,
                Source = "CHALLENGE",
                SourceRef = award.AwardId,
                CreatedAt = award.Timestamp
            });

            ledgerSequence++;
        }

        return result;
    }
}
