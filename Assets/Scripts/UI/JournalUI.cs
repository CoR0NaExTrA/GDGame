using UnityEngine;

public class JournalUI
{
    private Vector2 scroll;

    public void Draw(JournalSystem journal)
    {
        float width = Mathf.Min(760, Screen.width - 48);
        float height = Mathf.Min(620, Screen.height - 48);
        Rect panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        GUI.Box(panel, GUIContent.none, UITheme.Panel);
        GUI.Label(new Rect(panel.x + 28, panel.y + 22, width - 56, 42), "Дневник расследования", UITheme.Heading);
        GUI.Label(new Rect(panel.x + 28, panel.y + 64, width - 56, 30), "Улики и выбранные фрагменты разговоров", UITheme.Muted);

        Rect listArea = new Rect(panel.x + 24, panel.y + 102, width - 48, height - 154);
        GUILayout.BeginArea(listArea);
        scroll = GUILayout.BeginScrollView(scroll);
        System.Collections.Generic.IReadOnlyList<JournalEntry> entries = journal != null ? journal.Entries : null;
        if (entries == null || entries.Count == 0)
        {
            GUILayout.Label("Пока пусто. Осмотрите предмет или сохраните фрагмент разговора.", UITheme.Body, GUILayout.Height(74));
        }
        else
        {
            foreach (JournalEntry entry in entries)
            {
                GUILayout.BeginVertical(UITheme.Panel);
                GUILayout.Label(entry.title + "  ·  " + entry.kind, UITheme.Heading);
                GUILayout.Label("«" + entry.text + "»", UITheme.Body);
                GUILayout.Label("Источник: " + entry.source + "     " + entry.date, UITheme.Muted);
                GUILayout.EndVertical();
                GUILayout.Space(10);
            }
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
        GUI.Label(new Rect(panel.x + 28, panel.y + height - 38, width - 56, 24), "TAB — закрыть дневник", UITheme.Muted);
    }
}
