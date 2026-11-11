using UnityEngine;

public class WorldSystem : MonoBehaviour
{
    private SessionData subscribedData;

    private void Start()
    {
        SubscribeToSession();
        EnsureNightEvidence();
        RefreshWorld();
    }

    private void OnDestroy()
    {
        if (subscribedData != null) subscribedData.Changed -= RefreshWorld;
    }

    public bool IsAvailable(InteractableObject target)
    {
        if (target == null || GameSession.Instance == null) return false;
        SessionData data = GameSession.Instance.Data;
        if (!target.IsAvailableForPeriod(data.Period)) return false;
        if (!string.IsNullOrEmpty(target.CollectedFlagId) && data.GetFlag(target.CollectedFlagId)) return false;
        if (!string.IsNullOrEmpty(target.RequiredFlagId) && data.GetFlag(target.RequiredFlagId) != target.RequiredFlagValue) return false;
        return true;
    }

    public void RefreshWorld()
    {
        if (GameSession.Instance == null) return;
        InteractableObject[] objects = FindObjectsByType<InteractableObject>(FindObjectsInactive.Include);
        foreach (InteractableObject sceneObject in objects)
            sceneObject.gameObject.SetActive(IsAvailable(sceneObject));
    }

    private void EnsureNightEvidence()
    {
        InteractableObject[] existing = FindObjectsByType<InteractableObject>(FindObjectsInactive.Include);
        foreach (InteractableObject item in existing)
            if (item.Kind == InteractableKind.NightEvidence) return;

        InteractableObject dayItem = null;
        foreach (InteractableObject item in existing)
            if (item.Kind == InteractableKind.Item) { dayItem = item; break; }
        if (dayItem == null) return;

        SpriteRenderer source = dayItem.GetComponent<SpriteRenderer>();
        if (source == null || source.sprite == null) return;

        bool archive = dayItem.gameObject.scene.name == "ArchiveScene";
        string location = dayItem.gameObject.scene.name;
        GameObject clue = new GameObject("NightEvidence");
        clue.transform.position = dayItem.transform.position + new Vector3(2.5f, 1.5f, 0f);
        clue.transform.localScale = dayItem.transform.localScale;
        SpriteRenderer renderer = clue.AddComponent<SpriteRenderer>();
        renderer.sprite = source.sprite;
        renderer.color = new Color(0.3f, 0.8f, 1f, 1f);
        renderer.sortingOrder = source.sortingOrder + 1;
        InteractableObject data = clue.AddComponent<InteractableObject>();
        data.ConfigureRuntime(
            "night_evidence_" + location,
            InteractableKind.NightEvidence,
            archive ? "Ночная улика в архиве" : "Ночная улика",
            15,
            false,
            true,
            "item_night_" + location + "_collected",
            null,
            archive ? "Ночная улика в архиве" : "Ночная улика",
            archive
                ? "Одна архивная папка лежит не на своём месте; внутри — копия ключевого журнала."
                : "На подоконнике осталась влажная земля. След ведёт к запасному выходу.");
    }

    private void SubscribeToSession()
    {
        if (GameSession.Instance == null || subscribedData == GameSession.Instance.Data) return;
        if (subscribedData != null) subscribedData.Changed -= RefreshWorld;
        subscribedData = GameSession.Instance.Data;
        subscribedData.Changed += RefreshWorld;
    }
}
