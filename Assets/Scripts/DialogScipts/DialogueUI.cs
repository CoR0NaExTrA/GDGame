using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace DialogueSystems
{
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] private DialogueManager _manager;
        [SerializeField] private GameObject _dialoguePanel;
        [SerializeField] private TextMeshProUGUI _textDisplayer;
        [SerializeField] private TextMeshProUGUI _nameDisplayer;
        [SerializeField] private Image _portraitDisplayer;
        [SerializeField] private Transform _choicesContainer;
        [SerializeField] private Button _choiceButtonTemplate;

        private readonly List<Button> _choiceButtons = new();
        public event Action<int> ChoiceSelected;


        public void Show()
        {
            _dialoguePanel.SetActive(true);
        }

        public void Hide()
        {
            _dialoguePanel.SetActive(false);
        }

        public void DisplayLine(DialogueLine line)
        {
            var Speaker = line.Speaker;
            _nameDisplayer.text = Speaker.Name;
            _portraitDisplayer.sprite = Speaker.Portrait;
            _textDisplayer.text = line.Text;
        }

        public void DisplayChoices(IReadOnlyList<DialogueChoice> choices)
        {
            _choicesContainer.gameObject.SetActive(true);
            while (_choiceButtons.Count < choices.Count)
            {
                Button button = Instantiate(_choiceButtonTemplate, _choicesContainer);
                _choiceButtons.Add(button);
            }

            for (int i = 0; i < _choiceButtons.Count; i++)
            {
                Button button = _choiceButtons[i];

                if (i < choices.Count)
                {
                    button.gameObject.SetActive(true);
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
                int index = i;
                button.GetComponentInChildren<TMP_Text>().text = choices[i].Text;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => ChoiceSelected?.Invoke(index));
                button.onClick.AddListener(() => _choicesContainer.gameObject.SetActive(false));
            }
        }

        public void OnAdvanceButtonClicked()
        {
            _manager.ProccessLine();
        }

        public void DisableChoices()
        {
            foreach (Button button in _choiceButtons)
            {
                button.interactable = false;
            }
        }

        private void Awake()
        {
            _choiceButtons.Add(_choiceButtonTemplate);

            _choicesContainer.gameObject.SetActive(false);
            _dialoguePanel.SetActive(false);
        }
    }
}

