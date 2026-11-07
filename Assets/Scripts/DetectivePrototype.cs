using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Small scene prototype: proximity interactions, conversations, collectible clues and a notebook.
/// The current scene uses placeholder dialogue and clue text that can be replaced in ConfigureTargets.
/// </summary>
public class DetectivePrototype : MonoBehaviour
{
    [Serializable]
    private class JournalEntry
    {
        public string id;
        public string title;
        public string text;
        public string source;
        public string kind;
        public string date;
    }

    [Serializable]
    private class JournalSave
    {
        public List<JournalEntry> entries = new List<JournalEntry>();
    }

    [Serializable]
    private class TimeSave
    {
        public int energy;
        public int dayNumber;
        public bool isNight;
    }

    private class DialogueLine
    {
        public string speaker;
        public string text;
        public string clueFragment;

        public DialogueLine(string speaker, string text, string clueFragment = null)
        {
            this.speaker = speaker;
            this.text = text;
            this.clueFragment = clueFragment;
        }
    }

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 2.25f;
    [SerializeField] private Key journalKey = Key.Tab;
    [SerializeField] private Key interactKey = Key.E;

    private readonly List<JournalEntry> entries = new List<JournalEntry>();
    private readonly List<DialogueLine> keyDialogue = new List<DialogueLine>();
    private readonly List<string> secondaryDialogue = new List<string>();
    private Transform keyNpc;
    private Transform secondaryNpc;
    private Transform item;
    private Transform nightEvidence;
    private Transform nearbyTarget;
    private PlayerMovement movement;
    private Rigidbody2D body;
    private bool journalOpen;
    private bool dialogueOpen;
    private bool keyConversation;
    private bool itemCollected;
    private bool nightEvidenceCollected;
    private bool isNight;
    private int dayNumber = 1;
    private int energy = 100;
    private int dialogueIndex;
    private bool mapOpen;
    private string toastMessage;
    private float toastUntil;
    private Vector2 journalScroll;

    private GUIStyle panelStyle;
    private GUIStyle headingStyle;
    private GUIStyle bodyStyle;
    private GUIStyle mutedStyle;
    private GUIStyle buttonStyle;
    private GUIStyle toastStyle;

    private const string SaveKey = "DetectivePrototype.Journal.v1";
    private const string TimeSaveKey = "DetectivePrototype.Time.v1";
    private const int MaxEnergy = 100;
    private const int TalkKeyCost = 25;
    private const int TalkSecondaryCost = 10;
    private const int ItemCost = 15;
    private const int TravelCost = 20;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        body = GetComponent<Rigidbody2D>();
        ConfigureTargets();
        LoadJournal();
        LoadTimeState();
        ApplyWorldPhase();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        bool controlHeld = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;
        if (controlHeld && Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetDemo();
            return;
        }

        if (Keyboard.current[journalKey].wasPressedThisFrame)
        {
            journalOpen = !journalOpen;
            mapOpen = false;
            dialogueOpen = false;
            SetMovementEnabled(!journalOpen && !mapOpen);
        }

        if (!journalOpen && Keyboard.current.mKey.wasPressedThisFrame)
        {
            mapOpen = !mapOpen;
            dialogueOpen = false;
            SetMovementEnabled(!mapOpen);
        }

        if (journalOpen || mapOpen) return;

        FindNearbyTarget();
        if (Keyboard.current[interactKey].wasPressedThisFrame)
        {
            if (dialogueOpen)
                AdvanceDialogue();
            else if (nearbyTarget != null)
                InteractWith(nearbyTarget);
        }
        else if (dialogueOpen && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            AdvanceDialogue();
        }
    }

    private void ConfigureTargets()
    {
        foreach (Transform candidate in FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            switch (candidate.name)
            {
                case "KeyNPC": keyNpc = candidate; break;
                case "NonKeyNPC": secondaryNpc = candidate; break;
                case "Item": item = candidate; break;
            }
        }

        CreateNightEvidence();

        keyDialogue.Add(new DialogueLine("Вы", "Вы были здесь вчера вечером?"));
        bool inArchive = SceneManager.GetActiveScene().name == "ArchiveScene";
        string keyNpcName = inArchive ? "Архивариус" : "Марта";
        keyDialogue.Add(new DialogueLine(keyNpcName,
            inArchive ? "Я закрывал архив после девяти." : "Я закрывала комнату после девяти.",
            inArchive ? "Архив закрыли после девяти." : "После девяти я закрыла комнату."));
        keyDialogue.Add(new DialogueLine("Вы", "Кто-нибудь заходил после этого?"));
        keyDialogue.Add(new DialogueLine(keyNpcName,
            inArchive ? "Я слышал шаги между стеллажами, но никого не видел." : "Слышала шаги у восточной двери, но никого не видела.",
            inArchive ? "Шаги слышали между стеллажами." : "Слышала шаги у восточной двери."));

        secondaryDialogue.Add("Я ничего не знаю. Просто проходил мимо.");
        secondaryDialogue.Add("Кажется, здесь кто-то оставил эту вещь.");
    }

    private void FindNearbyTarget()
    {
        nearbyTarget = null;
        float bestDistance = interactionDistance;
        ConsiderTarget(keyNpc, ref bestDistance);
        ConsiderTarget(secondaryNpc, ref bestDistance);
        if (!itemCollected && !isNight) ConsiderTarget(item, ref bestDistance);
        if (!nightEvidenceCollected && isNight) ConsiderTarget(nightEvidence, ref bestDistance);
    }

    private void ConsiderTarget(Transform candidate, ref float bestDistance)
    {
        if (candidate == null || !candidate.gameObject.activeInHierarchy) return;
        float distance = Vector2.Distance(transform.position, candidate.position);
        if (distance <= bestDistance)
        {
            bestDistance = distance;
            nearbyTarget = candidate;
        }
    }

    private void InteractWith(Transform target)
    {
        if (target == item)
        {
            itemCollected = true;
            bool inArchive = SceneManager.GetActiveScene().name == "ArchiveScene";
            AddEntry(DayItemId(), inArchive ? "Архивная карточка" : "Найденная записка",
                inArchive ? "На карточке стоит отметка о выдаче ключа после закрытия архива." : "На клочке бумаги виден след синей краски.",
                inArchive ? "Осмотр архива" : "Осмотр комнаты", "Предмет");
            SpendTime(ItemCost);
            ApplyWorldPhase();
            return;
        }

        if (target == nightEvidence)
        {
            nightEvidenceCollected = true;
            bool inArchive = SceneManager.GetActiveScene().name == "ArchiveScene";
            AddEntry(NightEvidenceId(), inArchive ? "Ночная улика в архиве" : "Ночная улика",
                inArchive ? "Одна архивная папка лежит не на своём месте; внутри — копия ключевого журнала." : "На подоконнике осталась влажная земля. След ведёт к запасному выходу.",
                inArchive ? "Ночной осмотр архива" : "Ночной осмотр комнаты", "Улика");
            SpendTime(ItemCost);
            ApplyWorldPhase();
            return;
        }

        dialogueOpen = true;
        keyConversation = target == keyNpc;
        dialogueIndex = 0;
        SpendTime(keyConversation ? TalkKeyCost : TalkSecondaryCost);
        ApplyWorldPhase();
        SetMovementEnabled(false);
    }

    private void AdvanceDialogue()
    {
        dialogueIndex++;
        int lineCount = keyConversation ? keyDialogue.Count : secondaryDialogue.Count;
        if (dialogueIndex >= lineCount)
        {
            dialogueOpen = false;
            SetMovementEnabled(true);
        }
    }

    private void AddEntry(string id, string title, string text, string source, string kind)
    {
        if (entries.Exists(entry => entry.id == id))
        {
            ShowToast("Эта запись уже есть в дневнике");
            return;
        }

        entries.Insert(0, new JournalEntry
        {
            id = id,
            title = title,
            text = text,
            source = source,
            kind = kind,
            date = DateTime.Now.ToString("dd.MM.yyyy HH:mm")
        });
        SaveJournal();
        ShowToast("Запись добавлена в дневник");
    }

    private void LoadJournal()
    {
        string json = PlayerPrefs.GetString(SaveKey, string.Empty);
        if (string.IsNullOrEmpty(json)) return;
        try
        {
            JournalSave save = JsonUtility.FromJson<JournalSave>(json);
            if (save != null && save.entries != null) entries.AddRange(save.entries);
            itemCollected = entries.Exists(entry => entry.id == DayItemId());
            nightEvidenceCollected = entries.Exists(entry => entry.id == NightEvidenceId());
            if (itemCollected && item != null) item.gameObject.SetActive(false);
            if (nightEvidenceCollected && nightEvidence != null) nightEvidence.gameObject.SetActive(false);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Could not load the detective journal: " + exception.Message);
        }
    }

    private void SaveJournal()
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(new JournalSave { entries = entries }));
        PlayerPrefs.Save();
    }

    private void CreateNightEvidence()
    {
        if (item == null) return;
        SpriteRenderer sourceRenderer = item.GetComponent<SpriteRenderer>();
        if (sourceRenderer == null || sourceRenderer.sprite == null) return;

        GameObject evidenceObject = new GameObject("NightEvidence");
        evidenceObject.transform.position = item.position + new Vector3(2.5f, 1.5f, 0f);
        evidenceObject.transform.localScale = item.localScale;
        SpriteRenderer evidenceRenderer = evidenceObject.AddComponent<SpriteRenderer>();
        evidenceRenderer.sprite = sourceRenderer.sprite;
        evidenceRenderer.color = new Color(0.3f, 0.8f, 1f, 1f);
        evidenceRenderer.sortingOrder = sourceRenderer.sortingOrder + 1;
        nightEvidence = evidenceObject.transform;
        evidenceObject.SetActive(false);
    }

    private void LoadTimeState()
    {
        if (!PlayerPrefs.HasKey(TimeSaveKey)) return;
        string json = PlayerPrefs.GetString(TimeSaveKey, string.Empty);
        if (string.IsNullOrEmpty(json)) return;
        try
        {
            TimeSave save = JsonUtility.FromJson<TimeSave>(json);
            if (save == null) return;
            energy = Mathf.Clamp(save.energy, 1, MaxEnergy);
            dayNumber = Mathf.Max(1, save.dayNumber);
            isNight = save.isNight;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Could not load the detective time state: " + exception.Message);
        }
    }

    private void SaveTimeState()
    {
        PlayerPrefs.SetString(TimeSaveKey, JsonUtility.ToJson(new TimeSave
        {
            energy = energy,
            dayNumber = dayNumber,
            isNight = isNight
        }));
        PlayerPrefs.Save();
    }

    private void SpendTime(int amount)
    {
        int remainingEnergy = energy - amount;
        while (remainingEnergy <= 0)
        {
            AdvancePeriod();
            remainingEnergy += MaxEnergy;
        }
        energy = remainingEnergy;
        SaveTimeState();
    }

    private void AdvancePeriod()
    {
        if (isNight)
        {
            isNight = false;
            dayNumber++;
            ShowToast("Наступил день " + dayNumber);
        }
        else
        {
            isNight = true;
            ShowToast("Наступила ночь");
        }
    }

    private void ApplyWorldPhase()
    {
        if (secondaryNpc != null) secondaryNpc.gameObject.SetActive(!isNight);
        if (item != null) item.gameObject.SetActive(!isNight && !itemCollected);
        if (nightEvidence != null) nightEvidence.gameObject.SetActive(isNight && !nightEvidenceCollected);
        SaveTimeState();
    }

    private string DayItemId()
    {
        return "item:day:" + SceneManager.GetActiveScene().name;
    }

    private string NightEvidenceId()
    {
        return "item:night:" + SceneManager.GetActiveScene().name;
    }

    private void TravelTo(string sceneName)
    {
        if (SceneManager.GetActiveScene().name == sceneName)
        {
            mapOpen = false;
            SetMovementEnabled(true);
            return;
        }

        SpendTime(TravelCost);
        SceneManager.LoadScene(sceneName);
    }

    private void ResetDemo()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.DeleteKey(TimeSaveKey);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void SetMovementEnabled(bool enabled)
    {
        if (movement != null) movement.enabled = enabled;
        if (!enabled && body != null) body.linearVelocity = Vector2.zero;
    }

    private void ShowToast(string message)
    {
        toastMessage = message;
        toastUntil = Time.unscaledTime + 2.5f;
    }

    private void OnGUI()
    {
        EnsureStyles();
        DrawHud();
        if (dialogueOpen) DrawDialogue();
        if (journalOpen) DrawJournal();
        if (mapOpen) DrawLocationMap();
        if (Time.unscaledTime < toastUntil) DrawToast();
    }

    private void DrawHud()
    {
        GUI.Label(new Rect(24, 20, 500, 36), "E  Взаимодействовать     TAB  Дневник     Ctrl+R  Сброс", mutedStyle);
        string periodName = isNight ? "Ночь" : "День";
        GUI.Label(new Rect(24, 57, 420, 30), "День " + dayNumber + " · " + periodName + " · " + CurrentLocationName(), headingStyle);
        GUI.Label(new Rect(24, 91, 230, 26), "Энергия: " + energy + " / " + MaxEnergy, mutedStyle);
        GUI.Box(new Rect(24, 119, 220, 15), GUIContent.none, panelStyle);
        Color previousColor = GUI.color;
        GUI.color = isNight ? new Color(0.35f, 0.72f, 0.92f) : new Color(0.91f, 0.68f, 0.3f);
        GUI.Box(new Rect(27, 122, 214f * energy / MaxEnergy, 9), GUIContent.none);
        GUI.color = previousColor;
        GUI.Label(new Rect(24, 146, 420, 26), "M  Перемещение между локациями", mutedStyle);
        if (nearbyTarget != null && !dialogueOpen && !journalOpen && !mapOpen)
            GUI.Label(new Rect(24, 180, 420, 32), "[E] " + TargetPrompt(nearbyTarget), headingStyle);
    }

    private string CurrentLocationName()
    {
        return SceneManager.GetActiveScene().name == "ArchiveScene" ? "Архив" : "Комната";
    }

    private void DrawLocationMap()
    {
        float width = Mathf.Min(560, Screen.width - 48);
        Rect panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - 280) * 0.5f, width, 280);
        GUI.Box(panel, GUIContent.none, panelStyle);
        GUI.Label(new Rect(panel.x + 28, panel.y + 24, width - 56, 36), "Выберите локацию", headingStyle);
        GUI.Label(new Rect(panel.x + 28, panel.y + 64, width - 56, 28), "Переход занимает " + TravelCost + " энергии.", mutedStyle);

        if (GUI.Button(new Rect(panel.x + 28, panel.y + 112, width - 56, 48), "Комната расследования", buttonStyle))
            TravelTo("TestScene");
        if (GUI.Button(new Rect(panel.x + 28, panel.y + 172, width - 56, 48), "Городской архив", buttonStyle))
            TravelTo("ArchiveScene");
        GUI.Label(new Rect(panel.x + 28, panel.y + 232, width - 56, 28), "M — закрыть карту", mutedStyle);
    }

    private string TargetPrompt(Transform target)
    {
        if (target == item) return "Осмотреть предмет  (−" + ItemCost + " энергии)";
        if (target == nightEvidence) return "Осмотреть ночную улику  (−" + ItemCost + " энергии)";
        if (target == keyNpc) return "Поговорить с " + (SceneManager.GetActiveScene().name == "ArchiveScene" ? "архивариусом" : "Мартой") + "  (−" + TalkKeyCost + " энергии)";
        return "Поговорить  (−" + TalkSecondaryCost + " энергии)";
    }

    private void DrawDialogue()
    {
        float width = Mathf.Min(760, Screen.width - 48);
        Rect panel = new Rect((Screen.width - width) * 0.5f, Screen.height - 245, width, 205);
        GUI.Box(panel, GUIContent.none, panelStyle);

        if (keyConversation)
        {
            DialogueLine line = keyDialogue[dialogueIndex];
            Rect portraitRect = new Rect(panel.x + 20, panel.y + 22, 56, 56);
            SpriteRenderer speakerRenderer = line.speaker == "Вы"
                ? GetComponent<SpriteRenderer>()
                : keyNpc != null ? keyNpc.GetComponent<SpriteRenderer>() : null;
            DrawPortrait(portraitRect, speakerRenderer != null ? speakerRenderer.sprite : null);
            GUI.Label(new Rect(panel.x + 90, panel.y + 22, width - 120, 30), line.speaker, headingStyle);
            GUI.Label(new Rect(panel.x + 90, panel.y + 58, width - 120, 62), line.text, bodyStyle);

            if (!string.IsNullOrEmpty(line.clueFragment))
            {
                GUI.Label(new Rect(panel.x + 24, panel.y + 127, 118, 25), "Важный фрагмент", mutedStyle);
                GUI.Label(new Rect(panel.x + 155, panel.y + 127, width - 330, 30), "«" + line.clueFragment + "»", bodyStyle);
                if (GUI.Button(new Rect(panel.x + width - 165, panel.y + 124, 140, 34), "Записать", buttonStyle))
                    AddEntry("quote:" + SceneManager.GetActiveScene().name + ":" + dialogueIndex, "Фрагмент разговора", line.clueFragment, line.speaker, "Свидетельство");
            }
        }
        else
        {
            GUI.Label(new Rect(panel.x + 24, panel.y + 24, width - 48, 24), "Разговор", mutedStyle);
            GUI.Label(new Rect(panel.x + 24, panel.y + 66, width - 48, 82), secondaryDialogue[dialogueIndex], bodyStyle);
        }

        GUI.Label(new Rect(panel.x + 24, panel.y + 169, width - 48, 25), "E / Пробел — дальше", mutedStyle);
    }

    private void DrawPortrait(Rect rect, Sprite sprite)
    {
        GUI.Box(rect, GUIContent.none, buttonStyle);
        if (sprite == null) return;

        Rect textureRect = sprite.textureRect;
        Rect uv = new Rect(
            textureRect.x / sprite.texture.width,
            textureRect.y / sprite.texture.height,
            textureRect.width / sprite.texture.width,
            textureRect.height / sprite.texture.height);
        GUI.DrawTextureWithTexCoords(rect, sprite.texture, uv, true);
    }

    private void DrawJournal()
    {
        float width = Mathf.Min(760, Screen.width - 48);
        float height = Mathf.Min(620, Screen.height - 48);
        Rect panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        GUI.Box(panel, GUIContent.none, panelStyle);
        GUI.Label(new Rect(panel.x + 28, panel.y + 22, width - 56, 42), "Дневник расследования", headingStyle);
        GUI.Label(new Rect(panel.x + 28, panel.y + 64, width - 56, 30), "Улики и выбранные фрагменты разговоров", mutedStyle);

        Rect listRect = new Rect(panel.x + 24, panel.y + 102, width - 48, height - 154);
        GUILayout.BeginArea(listRect);
        journalScroll = GUILayout.BeginScrollView(journalScroll);
        if (entries.Count == 0)
        {
            GUILayout.Label("Пока пусто. Осмотрите предмет или сохраните важную фразу ключевого свидетеля.", bodyStyle, GUILayout.Height(74));
        }
        else
        {
            foreach (JournalEntry entry in entries)
            {
                GUILayout.BeginVertical(panelStyle);
                GUILayout.Label(entry.title + "  ·  " + entry.kind, headingStyle);
                GUILayout.Label("«" + entry.text + "»", bodyStyle);
                GUILayout.Label("Источник: " + entry.source + "     " + entry.date, mutedStyle);
                GUILayout.EndVertical();
                GUILayout.Space(10);
            }
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
        GUI.Label(new Rect(panel.x + 28, panel.y + height - 38, width - 56, 24), "TAB — закрыть дневник", mutedStyle);
    }

    private void DrawToast()
    {
        float width = Mathf.Min(360, Screen.width - 40);
        GUI.Box(new Rect((Screen.width - width) * 0.5f, 24, width, 48), toastMessage, toastStyle);
    }

    private void EnsureStyles()
    {
        if (panelStyle != null) return;
        panelStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = MakeTexture(new Color(0.075f, 0.09f, 0.12f, 0.97f)) },
            border = new RectOffset(12, 12, 12, 12),
            padding = new RectOffset(16, 16, 12, 12)
        };
        headingStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.93f, 0.83f, 0.62f) } };
        bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true, normal = { textColor = new Color(0.91f, 0.92f, 0.93f) } };
        mutedStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, normal = { textColor = new Color(0.67f, 0.73f, 0.78f) } };
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 15, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
        toastStyle = new GUIStyle(panelStyle) { alignment = TextAnchor.MiddleCenter, fontSize = 16, fontStyle = FontStyle.Bold, normal = { background = MakeTexture(new Color(0.15f, 0.24f, 0.21f, 0.97f)), textColor = Color.white } };
    }

    private static Texture2D MakeTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
