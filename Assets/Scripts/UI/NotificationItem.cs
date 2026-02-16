using TMPro;
using UnityEngine;

public class NotificationItem : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;

    public void Bind(string message)
    {
        if (messageText != null)
        {
            messageText.text = string.IsNullOrWhiteSpace(message) ? "-" : message;
        }
    }
}
