using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

namespace DialogueSystems
{
    public class DialogueManager : MonoBehaviour
    {

        [SerializeField] private DialogueUI _dialogueUI;
        private DialogueNode _currentNode;
        private int _currentLineIndex;

        public void StartDialogue(DialogueNode entryNode)
        {
            //pause da game
            EnterNode(entryNode);
            _dialogueUI.Show();
        }

        private void EndDialogue()
        {
            _currentNode = null;
            _dialogueUI.Hide();
        }

        private void EnterNode(DialogueNode node)
        {
            //on start actions
            _currentNode = node;
            switch (_currentNode)
            {
                case DialogueSequenceNode sequenceNode:
                    _currentLineIndex = 0;
                    ProccessLine();
                    break;
                case DialogueChoiceNode choiceNode:
                    _dialogueUI.DisplayChoices(choiceNode.Choices);
                    _dialogueUI.ChoiceSelected += SelectChoice;
                    break;
            }
        }

        private void AdvanceDialogue(DialogueNode nextNode)
        {
            //on end actions

            if (nextNode == null)
            {
                EndDialogue();
                return;
            }

            EnterNode(nextNode);
        }

        public void ProccessLine()
        {          
            var node = (DialogueSequenceNode)_currentNode;

            if (_currentLineIndex >= node.Lines.Count)
            {
                AdvanceDialogue(node.NextNode);
                return;
            }

            var line = node.Lines[_currentLineIndex];
            _dialogueUI.DisplayLine(line);

            _currentLineIndex++;
        }

        public void SelectChoice(int choiceIndex)
        {
            UnityEngine.Debug.Log($"{choiceIndex}");
            if (_currentNode is not DialogueChoiceNode choiceNode)
                return;

            if (choiceIndex < 0 || choiceIndex >= choiceNode.Choices.Count)
                return;

            var selectedChoice = choiceNode.Choices[choiceIndex];

            _dialogueUI.ChoiceSelected -= SelectChoice;
            AdvanceDialogue(selectedChoice.NextNode);
        }
    }
}
