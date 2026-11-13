using System;

[Serializable]
public class JournalEntry
{
    public string id;
    public string title;
    public string text;
    public string source;
    public string kind;
    public string date;

    public JournalEntry() { }

    public JournalEntry(string id, string title, string text, string source, string kind)
    {
        this.id = id;
        this.title = title;
        this.text = text;
        this.source = source;
        this.kind = kind;
        date = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
    }

    public JournalEntry Clone()
    {
        return new JournalEntry
        {
            id = id,
            title = title,
            text = text,
            source = source,
            kind = kind,
            date = date
        };
    }
}
