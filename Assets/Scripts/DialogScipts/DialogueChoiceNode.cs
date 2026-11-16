using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueChoceNode", menuName = "Dialogue/DialogueChoiceNode")]
public class DialogueChoiceNode : DialogueNode
{
    [SerializeField] private List<DialogueChoice> _choices;
    public IReadOnlyList<DialogueChoice> Choices { get => _choices; }

}
