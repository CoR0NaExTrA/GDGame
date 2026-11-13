using System;

[Serializable]
public class FlagEntry
{
    public string id;
    public bool value;

    public FlagEntry() { }

    public FlagEntry(string id, bool value)
    {
        this.id = id;
        this.value = value;
    }
}
