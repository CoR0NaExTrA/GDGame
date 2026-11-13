using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;

    private InteractionSystem interactionSystem;
    private DialogueSystem dialogueSystem;
    private JournalSystem journalSystem;
    private TimeSystem timeSystem;
    private LocationSystem locationSystem;
    private bool journalOpen;
    private bool mapOpen;
    private readonly HUD hud = new HUD();
    private readonly DialogueUI dialogueUI = new DialogueUI();
    private readonly JournalUI journalUI = new JournalUI();
    private readonly LocationMapUI locationMapUI = new LocationMapUI();
    private readonly ToastUI toastUI = new ToastUI();

    private void Awake()
    {
        instance = this;
        interactionSystem = GetComponent<InteractionSystem>();
        dialogueSystem = GetComponent<DialogueSystem>();
        journalSystem = GetComponent<JournalSystem>();
        timeSystem = GetComponent<TimeSystem>();
        locationSystem = GetComponent<LocationSystem>();
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public static void ShowToast(string message)
    {
        if (instance != null) instance.toastUI.Show(message);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        bool controlHeld = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;
        if (controlHeld && Keyboard.current.nKey.wasPressedThisFrame)
        {
            if (GameSession.Instance != null) GameSession.Instance.StartNewGame();
            return;
        }
        if (controlHeld && Keyboard.current.rKey.wasPressedThisFrame)
        {
            if (GameSession.Instance != null) GameSession.Instance.StartNewGame();
            return;
        }
        if (controlHeld && Keyboard.current.sKey.wasPressedThisFrame)
        {
            bool saved = GameSession.Instance != null && GameSession.Instance.Save();
            ShowToast(saved ? "Игра сохранена" : "Не удалось сохранить игру");
            return;
        }
        if (controlHeld && Keyboard.current.lKey.wasPressedThisFrame)
        {
            SaveLoadStatus status = SaveLoadStatus.Missing;
            bool loaded = GameSession.Instance != null && GameSession.Instance.LoadGame(out status);
            if (!loaded)
            {
                if (GameSession.Instance == null) ShowToast("Система игры не готова");
                else if (status == SaveLoadStatus.Corrupt) ShowToast("Файл сохранения повреждён");
                else ShowToast("Сохранение не найдено");
            }
            return;
        }

        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            journalOpen = !journalOpen;
            mapOpen = false;
            UpdateModalState();
        }
        else if (!journalOpen && Keyboard.current.mKey.wasPressedThisFrame)
        {
            mapOpen = !mapOpen;
            UpdateModalState();
        }
    }

    private void UpdateModalState()
    {
        if (journalOpen || mapOpen)
            if (dialogueSystem != null) dialogueSystem.Cancel();
        if (interactionSystem != null) interactionSystem.SetModalLock(journalOpen || mapOpen);
    }

    private void OnGUI()
    {
        if (GameSession.Instance == null) return;
        SessionData data = GameSession.Instance.Data;
        hud.Draw(data, interactionSystem, locationSystem);
        if (dialogueSystem != null && dialogueSystem.IsActive)
            dialogueUI.Draw(dialogueSystem, journalSystem);
        if (journalOpen) journalUI.Draw(journalSystem);
        if (mapOpen) locationMapUI.Draw(locationSystem, timeSystem);
        toastUI.Draw();
    }
}

internal static class UITheme
{
    private static Texture2D panelTexture;
    private static GUIStyle panel;
    private static GUIStyle heading;
    private static GUIStyle body;
    private static GUIStyle muted;
    private static GUIStyle button;
    private static GUIStyle toast;

    public static GUIStyle Panel { get { Ensure(); return panel; } }
    public static GUIStyle Heading { get { Ensure(); return heading; } }
    public static GUIStyle Body { get { Ensure(); return body; } }
    public static GUIStyle Muted { get { Ensure(); return muted; } }
    public static GUIStyle Button { get { Ensure(); return button; } }
    public static GUIStyle Toast { get { Ensure(); return toast; } }

    private static void Ensure()
    {
        if (panel != null) return;
        panelTexture = MakeTexture(new Color(0.075f, 0.09f, 0.12f, 0.97f));
        panel = new GUIStyle(GUI.skin.box)
        {
            normal = { background = panelTexture },
            border = new RectOffset(12, 12, 12, 12),
            padding = new RectOffset(16, 16, 12, 12)
        };
        heading = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.93f, 0.83f, 0.62f) }
        };
        body = new GUIStyle(GUI.skin.label)
        {
            fontSize = 17,
            wordWrap = true,
            normal = { textColor = new Color(0.91f, 0.92f, 0.93f) }
        };
        muted = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            normal = { textColor = new Color(0.67f, 0.73f, 0.78f) }
        };
        button = new GUIStyle(GUI.skin.button)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        toast = new GUIStyle(panel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            normal = { background = MakeTexture(new Color(0.15f, 0.24f, 0.21f, 0.97f)), textColor = Color.white }
        };
    }

    private static Texture2D MakeTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
