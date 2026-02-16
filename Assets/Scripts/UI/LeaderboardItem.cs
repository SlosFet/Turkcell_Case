using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LeaderboardItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image userPhotoImage;
    [SerializeField] private TMP_Text fullNameText;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private Image badgeImage;
    [SerializeField] private Button detailsButton;

    private string userId;
    private UnityAction<string> onClick;

    public void Bind(
        string userIdValue,
        string fullName,
        int rank,
        int points,
        Sprite userPhoto,
        Sprite badgeSprite,
        UnityAction<string> onClickHandler)
    {
        userId = userIdValue;
        onClick = onClickHandler;

        if (fullNameText != null)
        {
            fullNameText.text = fullName;
        }

        if (rankText != null)
        {
            rankText.text = rank.ToString();
        }

        if (pointsText != null)
        {
            pointsText.text = points.ToString();
        }

        if (userPhotoImage != null)
        {
            userPhotoImage.sprite = userPhoto;
            userPhotoImage.enabled = userPhoto != null;
        }

        if (badgeImage != null)
        {
            badgeImage.sprite = badgeSprite;
            badgeImage.enabled = badgeSprite != null;
        }

        if (detailsButton != null)
        {
            detailsButton.onClick.RemoveAllListeners();
            detailsButton.onClick.AddListener(HandleClick);
        }
    }

    private void HandleClick()
    {
        onClick?.Invoke(userId);
    }
}
