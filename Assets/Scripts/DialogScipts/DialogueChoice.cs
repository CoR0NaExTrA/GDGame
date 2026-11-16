using UnityEngine;
using System;

[Serializable]
public struct DialogueChoice
{
    [field: SerializeField]
    public string Text { get; private set; }

    [field: SerializeField]
    public DialogueNode NextNode { get; private set; }
}
