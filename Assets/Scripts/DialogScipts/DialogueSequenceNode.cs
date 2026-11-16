using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogNode", menuName = "Dialogue/DialogueSequenceNode")]
public class DialogueSequenceNode : DialogueNode
{
    [SerializeField]
    private List<DialogueLine> _lines;
    public IReadOnlyList<DialogueLine> Lines {  get { return _lines; } }

    [field: SerializeField]
    public DialogueNode NextNode { get; private set; }


}
