using System;
using System.Collections.Generic;

public enum TimePeriod
{
    Day,
    Night
}

[Serializable]
public class GameData
{
    public int version = 1;
    public int energy = 100;
    public int dayNumber = 1;
    public TimePeriod period = TimePeriod.Day;
    public string currentLocation = "TestScene";
    public List<FlagEntry> flags = new List<FlagEntry>();
    public List<JournalEntry> journal = new List<JournalEntry>();

    public static GameData CreateNew(string initialLocation)
    {
        return new GameData
        {
            energy = 100,
            dayNumber = 1,
            period = TimePeriod.Day,
            currentLocation = string.IsNullOrEmpty(initialLocation) ? "TestScene" : initialLocation,
            flags = new List<FlagEntry>(),
            journal = new List<JournalEntry>()
        };
    }

    public GameData Clone()
    {
        GameData copy = new GameData
        {
            version = version,
            energy = energy,
            dayNumber = dayNumber,
            period = period,
            currentLocation = currentLocation,
            flags = new List<FlagEntry>(),
            journal = new List<JournalEntry>()
        };

        if (flags != null)
            foreach (FlagEntry flag in flags)
                if (flag != null) copy.flags.Add(new FlagEntry(flag.id, flag.value));
        if (journal != null)
            foreach (JournalEntry entry in journal)
                if (entry != null) copy.journal.Add(entry.Clone());
        return copy;
    }

    public void Normalize()
    {
        if (flags == null) flags = new List<FlagEntry>();
        if (journal == null) journal = new List<JournalEntry>();
        flags.RemoveAll(flag => flag == null || string.IsNullOrEmpty(flag.id));
        journal.RemoveAll(entry => entry == null || string.IsNullOrEmpty(entry.id));
        if (string.IsNullOrEmpty(currentLocation)) currentLocation = "TestScene";
        energy = Math.Max(0, Math.Min(100, energy));
        dayNumber = Math.Max(1, dayNumber);
    }
}
