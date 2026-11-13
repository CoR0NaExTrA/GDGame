using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>One persistent owner of the active session and its save lifecycle.</summary>
public sealed class GameSession : MonoBehaviour
{
    private static GameSession instance;
    private SessionData sessionData;
    private bool loadedFromSave;
    private bool suppressAutomaticSave;

    public static GameSession Instance { get { return instance; } }
    public SessionData Data { get { return sessionData; } }
    public bool HasSavedGame { get { return SaveSystem.HasSave(); } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null) return;
        GameObject sessionObject = new GameObject("GameSession");
        sessionObject.AddComponent<GameSession>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        string initialLocation = GetInitialSceneName();
        GameData loadedData;
        SaveLoadStatus loadStatus = SaveSystem.Load(out loadedData);
        loadedFromSave = loadStatus == SaveLoadStatus.Loaded;
        suppressAutomaticSave = loadStatus == SaveLoadStatus.Corrupt;
        sessionData = new SessionData(loadedFromSave ? loadedData : GameData.CreateNew(initialLocation));
        SubscribeToChanges();
        if (!suppressAutomaticSave) Save();
    }

    private void Start()
    {
        if (!loadedFromSave) return;
        string targetScene = IsSceneAvailable(sessionData.CurrentLocation)
            ? sessionData.CurrentLocation
            : GetDefaultSceneName();
        if (sessionData.CurrentLocation != targetScene) sessionData.SetCurrentLocation(targetScene);
        if (SceneManager.GetActiveScene().name != targetScene) SceneManager.LoadScene(targetScene);
    }

    private void OnDestroy()
    {
        if (instance != this) return;
        if (sessionData != null) sessionData.Changed -= OnSessionChanged;
        instance = null;
    }

    public bool Save()
    {
        if (sessionData == null) return false;
        bool saved = SaveSystem.Save(sessionData.CreateSaveSnapshot());
        if (saved) suppressAutomaticSave = false;
        return saved;
    }

    public void StartNewGame()
    {
        if (sessionData != null) sessionData.Changed -= OnSessionChanged;
        sessionData = new SessionData(GameData.CreateNew(GetDefaultSceneName()));
        suppressAutomaticSave = false;
        SubscribeToChanges();
        Save();
        SceneManager.LoadScene(sessionData.CurrentLocation);
    }

    public bool LoadGame(out SaveLoadStatus loadStatus)
    {
        GameData loadedData;
        loadStatus = SaveSystem.Load(out loadedData);
        if (loadStatus != SaveLoadStatus.Loaded) return false;
        sessionData.Changed -= OnSessionChanged;
        sessionData = new SessionData(loadedData);
        suppressAutomaticSave = false;
        SubscribeToChanges();
        string targetScene = IsSceneAvailable(sessionData.CurrentLocation)
            ? sessionData.CurrentLocation
            : GetDefaultSceneName();
        if (sessionData.CurrentLocation != targetScene) sessionData.SetCurrentLocation(targetScene);
        SceneManager.LoadScene(targetScene);
        return true;
    }

    private void SubscribeToChanges()
    {
        sessionData.Changed += OnSessionChanged;
    }

    private void OnSessionChanged()
    {
        if (!suppressAutomaticSave) Save();
    }

    private static string GetInitialSceneName()
    {
        string activeName = SceneManager.GetActiveScene().name;
        if (!string.IsNullOrEmpty(activeName)) return activeName;
        string firstScenePath = SceneUtility.GetScenePathByBuildIndex(0);
        string firstSceneName = Path.GetFileNameWithoutExtension(firstScenePath);
        return string.IsNullOrEmpty(firstSceneName) ? "TestScene" : firstSceneName;
    }

    private static string GetDefaultSceneName()
    {
        string firstScenePath = SceneUtility.GetScenePathByBuildIndex(0);
        string firstSceneName = Path.GetFileNameWithoutExtension(firstScenePath);
        if (!string.IsNullOrEmpty(firstSceneName)) return firstSceneName;
        string activeName = SceneManager.GetActiveScene().name;
        return string.IsNullOrEmpty(activeName) ? "TestScene" : activeName;
    }

    private static bool IsSceneAvailable(string sceneName)
    {
        return !string.IsNullOrEmpty(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName);
    }
}
