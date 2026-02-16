using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AddUserPanel : UI_Panel
{
    public static event Action<List<UsersData>, List<ActivityEventsData>> OnUserDataSubmitted;

    [Header("Actions")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button continueButton;

    [Header("User Inputs")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField cityInput;

    [Header("Activity Inputs")]
    [SerializeField] private List<ActivityInput> activityInputRows = new List<ActivityInput>();
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
            continueButton.onClick.AddListener(()=> gameObject.SetActive(false));
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
            if (row == null)
            {
                continue;
            }

            var hasFirst = TryParseInput(row.MessageInput, out var firstValue);
            var hasSecond = TryParseInput(row.ReactionInput, out var secondValue);
            var hasGroup = TryParseInput(row.GroupInput, out var groupValue);
            if (!hasFirst && !hasSecond)
            {
                continue;
            }

            var normalizedDate = NormalizeDate(row.date);
            if (string.IsNullOrWhiteSpace(normalizedDate))
            {
                normalizedDate = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            var activity = new ActivityEventsData
            {
                EventId = "E" + eventSequence,
                UserId = userId,
                Date = normalizedDate,
                Messages = firstValue,
                Reactions = secondValue,
                UniqueGroups = hasGroup ? Mathf.Max(0, groupValue) : defaultUniqueGroups
            };

            eventSequence++;
            createdActivities.Add(activity);
            createdActivityCount++;
        }

        Debug.Log($"AddUserPanel: Created user {user.UserId} and {createdActivityCount} activity rows.");
    }

    private void HandleContinueClicked()
    {
        Debug.Log($"AddUserPanel: Submitting Users={createdUsers.Count}, Activities={createdActivities.Count}");
        OnUserDataSubmitted?.Invoke(new List<UsersData>(createdUsers), new List<ActivityEventsData>(createdActivities));
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

    private static string NormalizeDate(string rawDate)
    {
        if (string.IsNullOrWhiteSpace(rawDate))
        {
            return string.Empty;
        }

        var value = rawDate.Trim();
        string[] formats =
        {
            "yyyy-MM-dd",
            "dd.MM.yyyy",
            "d.M.yyyy",
            "dd/MM/yyyy",
            "d/M/yyyy",
            "MM/dd/yyyy",
            "M/d/yyyy"
        };

        for (var i = 0; i < formats.Length; i++)
        {
            if (DateTime.TryParseExact(value, formats[i], CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                return parsed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
        }

        if (DateTime.TryParse(value, out var fallback))
        {
            return fallback.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        return string.Empty;
    }
}
