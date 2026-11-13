using UnityEngine;

public enum InteractableKind
{
    KeyNpc,
    SecondaryNpc,
    Item,
    NightEvidence
}

/// <summary>Scene-authored interaction metadata for an NPC or clue.</summary>
public class InteractableObject : MonoBehaviour
{
    [SerializeField] private string id;
    [SerializeField] private InteractableKind kind;
    [SerializeField] private string displayName;
    [SerializeField, Min(0)] private int energyCost;
    [SerializeField] private bool availableDuringDay = true;
    [SerializeField] private bool availableAtNight = true;
    [SerializeField] private string requiredFlagId;
    [SerializeField] private bool requiredFlagValue = true;
    [SerializeField] private string collectedFlagId;
    [SerializeField] private string completionFlagId;
    [SerializeField] private string journalTitle;
    [SerializeField, TextArea] private string journalText;

    public string Id { get { return id; } }
    public InteractableKind Kind { get { return kind; } }
    public string DisplayName { get { return displayName; } }
    public int EnergyCost { get { return energyCost; } }
    public string CollectedFlagId { get { return collectedFlagId; } }
    public string RequiredFlagId { get { return requiredFlagId; } }
    public bool RequiredFlagValue { get { return requiredFlagValue; } }
    public string CompletionFlagId { get { return completionFlagId; } }
    public string JournalTitle { get { return journalTitle; } }
    public string JournalText { get { return journalText; } }

    public bool IsAvailableForPeriod(TimePeriod period)
    {
        return period == TimePeriod.Day ? availableDuringDay : availableAtNight;
    }

    public void ConfigureRuntime(string objectId, InteractableKind objectKind, string name, int cost,
        bool daytime, bool nighttime, string collectedFlag, string completionFlag,
        string entryTitle, string entryText, string requiredFlag = null, bool requiredValue = true)
    {
        id = objectId;
        kind = objectKind;
        displayName = name;
        energyCost = Mathf.Max(0, cost);
        availableDuringDay = daytime;
        availableAtNight = nighttime;
        collectedFlagId = collectedFlag;
        completionFlagId = completionFlag;
        journalTitle = entryTitle;
        journalText = entryText;
        requiredFlagId = requiredFlag;
        requiredFlagValue = requiredValue;
    }
}
