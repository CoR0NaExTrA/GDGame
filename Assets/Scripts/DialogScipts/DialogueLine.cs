using System;
using UnityEngine;

[Serializable]
public struct DialogueLine
{
    [field : SerializeField, TextArea(3, 9)]
    public string Text {  get; private set; }

    [field: SerializeField]
    public CharacterData Speaker { get; private set; }
 
}
