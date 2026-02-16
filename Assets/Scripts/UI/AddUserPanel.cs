using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct DailyActivity
{
    public int Reactions;
    public int Messages;
}

public class AddUserPanel : UI_Panel
{
    [Header("UI References")]
    [SerializeField] private InputField nameInput;
    [SerializeField] private InputField cityInput;
    [SerializeField] private Button addDayButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform dayInputsContainer;
    [SerializeField] private DayActivityInput dayActivityInputPrefab;

    private Action<string, string, List<DailyActivity>> onSaveCallback;
    private readonly List<DayActivityInput> dayInputs = new List<DayActivityInput>();

    public void Initialize(Action<string, string, List<DailyActivity>> onSave)
    {
        this.onSaveCallback = onSave;
    }

    private void Start()
    {
        if (addDayButton != null)
        {
            addDayButton.onClick.AddListener(OnAddDayButtonClicked);
        }
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(OnSaveButtonClicked);
        }
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }
        
        // Start with one day input
        OnAddDayButtonClicked();
    }

    private void OnDestroy()
    {
        if (addDayButton != null)
        {
            addDayButton.onClick.RemoveListener(OnAddDayButtonClicked);
        }
        if (saveButton != null)
        {
            saveButton.onClick.RemoveListener(OnSaveButtonClicked);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
        }
    }

    private void OnAddDayButtonClicked()
    {
        var dayInput = Instantiate(dayActivityInputPrefab, dayInputsContainer);
        dayInputs.Add(dayInput);
    }

    private void OnSaveButtonClicked()
    {
        var dailyActivities = new List<DailyActivity>();
        foreach (var dayInput in dayInputs)
        {
            if (int.TryParse(dayInput.reactionInput.text, out int reactions) && int.TryParse(dayInput.messageInput.text, out int messages))
            {
                dailyActivities.Add(new DailyActivity { Reactions = reactions, Messages = messages });
            }
            else
            {
                Debug.LogError("Invalid input for reactions or messages. Please enter a valid number.");
                return;
            }
        }

        onSaveCallback?.Invoke(nameInput.text, cityInput.text, dailyActivities);
        ClearInputFields();
        Close();
    }

    private void ClearInputFields()
    {
        nameInput.text = "";
        cityInput.text = "";
        foreach (var dayInput in dayInputs)
        {
            Destroy(dayInput.gameObject);
        }
        dayInputs.Clear();
    }
}
