using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionSystem : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 2.25f;

    private PlayerMovement movement;
    private DialogueSystem dialogueSystem;
    private JournalSystem journalSystem;
    private TimeSystem timeSystem;
    private WorldSystem worldSystem;
    private bool modalLocked;
    private bool dialogueLocked;

    public InteractableObject CurrentTarget { get; private set; }

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        dialogueSystem = GetComponent<DialogueSystem>();
        journalSystem = GetComponent<JournalSystem>();
        timeSystem = GetComponent<TimeSystem>();
        worldSystem = GetComponent<WorldSystem>();
        if (dialogueSystem != null) dialogueSystem.Closed += OnDialogueClosed;
    }

    private void OnDestroy()
    {
        if (dialogueSystem != null) dialogueSystem.Closed -= OnDialogueClosed;
    }

    private void Update()
    {
        if (modalLocked) return;
        FindNearestTarget();
        if (Keyboard.current == null) return;

        if (dialogueSystem != null && dialogueSystem.IsActive)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
                dialogueSystem.Advance();
            return;
        }
        if (Keyboard.current.eKey.wasPressedThisFrame && CurrentTarget != null &&
            worldSystem != null && worldSystem.IsAvailable(CurrentTarget))
            Interact(CurrentTarget);
    }

    public void SetModalLock(bool value)
    {
        modalLocked = value;
        UpdateMovementState();
    }

    public void SetDialogueLock(bool value)
    {
        dialogueLocked = value;
        UpdateMovementState();
    }

    private void FindNearestTarget()
    {
        CurrentTarget = null;
        if (worldSystem == null) return;
        float bestDistance = interactionDistance;
        InteractableObject[] objects = FindObjectsByType<InteractableObject>();
        foreach (InteractableObject candidate in objects)
        {
            if (!worldSystem.IsAvailable(candidate)) continue;
            float distance = Vector2.Distance(transform.position, candidate.transform.position);
            if (distance > bestDistance) continue;
            bestDistance = distance;
            CurrentTarget = candidate;
        }
    }

    private void Interact(InteractableObject target)
    {
        if (target.Kind == InteractableKind.Item || target.Kind == InteractableKind.NightEvidence)
        {
            if (timeSystem == null || !timeSystem.SpendEnergy(target.EnergyCost)) return;
            if (journalSystem != null && !string.IsNullOrEmpty(target.JournalTitle))
                journalSystem.AddEntry(target.Id, target.JournalTitle, target.JournalText, target.DisplayName, "Улика");
            if (GameSession.Instance != null && !string.IsNullOrEmpty(target.CollectedFlagId))
                GameSession.Instance.Data.SetFlag(target.CollectedFlagId, true);
            if (worldSystem != null) worldSystem.RefreshWorld();
            return;
        }

        if (timeSystem == null || dialogueSystem == null || !timeSystem.SpendEnergy(target.EnergyCost)) return;
        dialogueSystem.Begin(target);
        SetDialogueLock(dialogueSystem.IsActive);
    }

    private void UpdateMovementState()
    {
        if (movement != null) movement.enabled = !(modalLocked || dialogueLocked);
    }

    private void OnDialogueClosed()
    {
        SetDialogueLock(false);
    }
}
