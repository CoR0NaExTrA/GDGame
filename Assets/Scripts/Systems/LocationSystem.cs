using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LocationSystem : MonoBehaviour
{
    public struct LocationOption
    {
        public string sceneName;
        public string displayName;

        public LocationOption(string sceneName, string displayName)
        {
            this.sceneName = sceneName;
            this.displayName = displayName;
        }
    }

    private static readonly LocationOption[] locations =
    {
        new LocationOption("TestScene", "Комната расследования"),
        new LocationOption("ArchiveScene", "Городской архив")
    };

    public string CurrentSceneName { get { return SceneManager.GetActiveScene().name; } }
    public string CurrentDisplayName { get { return CurrentSceneName == "ArchiveScene" ? "Городской архив" : "Комната расследования"; } }
    public IList<LocationOption> Locations { get { return Array.AsReadOnly(locations); } }

    public bool TravelTo(string sceneName, TimeSystem timeSystem)
    {
        if (string.IsNullOrEmpty(sceneName) || GameSession.Instance == null) return false;
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            UIManager.ShowToast("Локация недоступна в настройках сборки");
            return false;
        }
        if (sceneName == CurrentSceneName) return true;

        SessionData data = GameSession.Instance.Data;
        if (timeSystem == null || !timeSystem.SpendEnergy(20)) return false;
        data.SetCurrentLocation(sceneName);
        SceneManager.LoadScene(sceneName);
        return true;
    }
}
