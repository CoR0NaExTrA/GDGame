using System;
using System.Collections.Generic;

/// <summary>Controlled mutable view of one in-memory game session.</summary>
public sealed class SessionData
{
    private readonly GameData data;

    public event Action Changed;

    public int Energy { get { return data.energy; } }
    public int DayNumber { get { return data.dayNumber; } }
    public TimePeriod Period { get { return data.period; } }
    public string CurrentLocation { get { return data.currentLocation; } }
    public IReadOnlyList<JournalEntry> Journal
    {
        get
        {
            List<JournalEntry> copy = new List<JournalEntry>();
            foreach (JournalEntry entry in data.journal)
                if (entry != null) copy.Add(entry.Clone());
            return copy.AsReadOnly();
        }
    }

    public SessionData(GameData source)
    {
        data = source == null ? GameData.CreateNew("TestScene") : source.Clone();
        data.Normalize();
    }

    public bool GetFlag(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        for (int i = 0; i < data.flags.Count; i++)
            if (data.flags[i] != null && data.flags[i].id == id) return data.flags[i].value;
        return false;
    }

    public void SetFlag(string id, bool value)
    {
        if (string.IsNullOrEmpty(id)) return;
        for (int i = 0; i < data.flags.Count; i++)
        {
            if (data.flags[i] == null || data.flags[i].id != id) continue;
            if (data.flags[i].value == value) return;
            data.flags[i].value = value;
            NotifyChanged();
            return;
        }
        data.flags.Add(new FlagEntry(id, value));
        NotifyChanged();
    }

    public bool TryAddJournalEntry(JournalEntry entry)
    {
        if (entry == null || string.IsNullOrEmpty(entry.id)) return false;
        for (int i = 0; i < data.journal.Count; i++)
            if (data.journal[i] != null && data.journal[i].id == entry.id) return false;
        data.journal.Insert(0, entry.Clone());
        NotifyChanged();
        return true;
    }

    public void SetTime(int energy, int dayNumber, TimePeriod period)
    {
        data.energy = Math.Max(0, Math.Min(100, energy));
        data.dayNumber = Math.Max(1, dayNumber);
        data.period = period;
        NotifyChanged();
    }

    public void SetCurrentLocation(string location)
    {
        if (string.IsNullOrEmpty(location) || data.currentLocation == location) return;
        data.currentLocation = location;
        NotifyChanged();
    }

    public GameData CreateSaveSnapshot()
    {
        return data.Clone();
    }

    private void NotifyChanged()
    {
        Action handler = Changed;
        if (handler != null) handler();
    }
}
