using TMPro;
using UnityEngine;

public class UserMetricsItem : MonoBehaviour
{
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text messagesText;
    [SerializeField] private TMP_Text reactionsText;
    [SerializeField] private TMP_Text uniqueGroupsText;

    public void Bind(string eventId, string userId, string date, int messages, int reactions, int uniqueGroups)
    {
        if (dateText != null)
        {
            dateText.text = "Date: " + date;
        }

        if (messagesText != null)
        {
            messagesText.text = "Message count: " + messages.ToString();
        }

        if (reactionsText != null)
        {
            reactionsText.text = "Reaction count: " + reactions.ToString();
        }

        if (uniqueGroupsText != null)
        {
            uniqueGroupsText.text = "Unique groups: " + uniqueGroups.ToString();
        }
    }
}
