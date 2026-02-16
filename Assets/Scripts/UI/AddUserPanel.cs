using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AddUserPanel : UI_Panel
{
    [Serializable]
    public class ActivityInputRow
    {
        public string date;
        public TMP_InputField firstInput;
        public TMP_InputField secondInput;
    }

    [Header("Actions")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button continueButton;

    [Header("User Inputs")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField cityInput;

    [Header("Activity Inputs")]
    [SerializeField] private List<ActivityInputRow> activityInputRows = new List<ActivityInputRow>();
    [SerializeField] private int defaultUniqueGroups = 1;

    [Header("Events")]
    [SerializeField] private UnityEvent onContinue;

    [Header("Created Data (Debug)")]
    [SerializeField] private List<UsersData> createdUsers = new List<UsersData>();
    [SerializeField] private List<ActivityEventsData> createdActivities = new List<ActivityEventsData>();

    private int userSequence = 1;
    private int eventSequence = 1;

    public IReadOnlyList<UsersData> CreatedUsers => createdUsers;
    public IReadOnlyList<ActivityEventsData> CreatedActivities => createdActivities;

    private void Awake()
    {
        if (applyButton != null)
        {
            applyButton.onClick.RemoveAllListeners();
            applyButton.onClick.AddListener(HandleApplyClicked);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(HandleContinueClicked);
        }
    }

    private void HandleApplyClicked()
    {
        var userName = nameInput != null ? nameInput.text?.Trim() : string.Empty;
        var city = cityInput != null ? cityInput.text?.Trim() : string.Empty;

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(city))
        {
            Debug.LogWarning("AddUserPanel: Name and City are required before apply.");
            return;
        }

        var userId = "U" + userSequence;
        userSequence++;

        var user = new UsersData
        {
            UserId = userId,
            Name = userName,
            City = city
        };
        createdUsers.Add(user);

        var createdActivityCount = 0;
        for (var i = 0; i < activityInputRows.Count; i++)
        {
            var row = activityInputRows[i];
            if (row == null || string.IsNullOrWhiteSpace(row.date))
            {
                continue;
            }

            var hasFirst = TryParseInput(row.firstInput, out var firstValue);
            var hasSecond = TryParseInput(row.secondInput, out var secondValue);
            if (!hasFirst && !hasSecond)
            {
                continue;
            }

            var activity = new ActivityEventsData
            {
                EventId = "E" + eventSequence,
                UserId = userId,
                Date = row.date,
                Messages = firstValue,
                Reactions = secondValue,
                UniqueGroups = defaultUniqueGroups
            };

            eventSequence++;
            createdActivities.Add(activity);
            createdActivityCount++;
        }

        Debug.Log($"AddUserPanel: Created user {user.UserId} and {createdActivityCount} activity rows.");
    }

    private void HandleContinueClicked()
    {
        onContinue?.Invoke();
    }

    private static bool TryParseInput(TMP_InputField inputField, out int value)
    {
        value = 0;
        if (inputField == null)
        {
            return false;
        }

        var raw = inputField.text != null ? inputField.text.Trim() : string.Empty;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        return int.TryParse(raw, out value);
    }
}
