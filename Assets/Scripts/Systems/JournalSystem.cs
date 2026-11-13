using System.Collections.Generic;
using UnityEngine;

public class JournalSystem : MonoBehaviour
{
    public IReadOnlyList<JournalEntry> Entries
    {
        get { return GameSession.Instance == null ? new JournalEntry[0] : GameSession.Instance.Data.Journal; }
    }

    public bool AddEntry(string id, string title, string text, string source, string kind)
    {
        if (GameSession.Instance == null) return false;
        bool added = GameSession.Instance.Data.TryAddJournalEntry(new JournalEntry(id, title, text, source, kind));
        if (added) UIManager.ShowToast("Запись добавлена в дневник");
        else UIManager.ShowToast("Эта запись уже есть в дневнике");
        return added;
    }
}
