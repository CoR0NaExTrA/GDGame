using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogNode", menuName = "Dialog/DialogNode")]
public class DialogNode : ScriptableObject
{
    [SerializeField, TextArea(3, 9)] private string _text;
    [SerializeField] private List<string> _choices;
    [field: SerializeField] public CharacterData Speaker { get; private set; }

    public string Text { get => _text; }
    
}
