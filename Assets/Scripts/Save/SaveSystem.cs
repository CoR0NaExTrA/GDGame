using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public enum SaveLoadStatus
{
    Missing,
    Loaded,
    Corrupt
}

/// <summary>JSON persistence only; it has no references to scene or gameplay systems.</summary>
public static class SaveSystem
{
    private const string FileName = "detective-save.json";

    public static string SavePath
    {
        get { return Path.Combine(Application.persistentDataPath, FileName); }
    }

    public static bool HasSave()
    {
        return File.Exists(SavePath);
    }

    public static bool Save(GameData data)
    {
        if (data == null) return false;
        try
        {
            data.Normalize();
            string json = JsonUtility.ToJson(data, true);
            string temporaryPath = SavePath + ".tmp";
            File.WriteAllText(temporaryPath, json);
            if (File.Exists(SavePath)) File.Replace(temporaryPath, SavePath, null);
            else File.Move(temporaryPath, SavePath);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("Could not save the game: " + exception.Message);
            return false;
        }
    }

    public static SaveLoadStatus Load(out GameData data)
    {
        data = null;
        if (!HasSave()) return SaveLoadStatus.Missing;
        try
        {
            string json = File.ReadAllText(SavePath);
            string trimmedJson = json.Trim();
            if (trimmedJson.Length < 2 || trimmedJson[0] != '{' || trimmedJson[trimmedJson.Length - 1] != '}' ||
                !HasProperty(trimmedJson, "version") || !HasProperty(trimmedJson, "energy") ||
                !HasProperty(trimmedJson, "dayNumber") || !HasProperty(trimmedJson, "period") ||
                !HasProperty(trimmedJson, "currentLocation") || !HasProperty(trimmedJson, "flags") ||
                !HasProperty(trimmedJson, "journal"))
                throw new InvalidDataException("Save file is missing required fields or is not a JSON object.");

            GameData loaded = JsonUtility.FromJson<GameData>(trimmedJson);
            if (loaded == null || loaded.version != 1 || loaded.energy < 0 || loaded.energy > 100 ||
                loaded.dayNumber < 1 || !Enum.IsDefined(typeof(TimePeriod), loaded.period) ||
                string.IsNullOrWhiteSpace(loaded.currentLocation) || loaded.flags == null || loaded.journal == null)
                throw new InvalidDataException("Save file contains invalid game data.");

            foreach (FlagEntry flag in loaded.flags)
                if (flag == null || string.IsNullOrWhiteSpace(flag.id))
                    throw new InvalidDataException("Save file contains an invalid flag.");
            foreach (JournalEntry entry in loaded.journal)
                if (entry == null || string.IsNullOrWhiteSpace(entry.id))
                    throw new InvalidDataException("Save file contains an invalid journal entry.");

            loaded.Normalize();
            data = loaded;
            return SaveLoadStatus.Loaded;
        }
        catch (Exception exception)
        {
            Debug.LogError("Could not load the game save: " + exception.Message);
            return SaveLoadStatus.Corrupt;
        }
    }

    private static bool HasProperty(string json, string propertyName)
    {
        return Regex.IsMatch(json, "\\\"" + Regex.Escape(propertyName) + "\\\"\\s*:");
    }

    public static bool DeleteSave()
    {
        try
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
            string temporaryPath = SavePath + ".tmp";
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("Could not delete the game save: " + exception.Message);
            return false;
        }
    }
}
