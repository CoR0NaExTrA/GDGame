using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSystem : MonoBehaviour
{
    [Serializable]
    public class DialogueChoice
    {
        public string text;
        public string targetNodeId;
        public string requiredFlagId;
        public bool requiredFlagValue = true;

        public bool IsAvailable(SessionData data)
        {
            return string.IsNullOrEmpty(requiredFlagId) ||
                   (data != null && data.GetFlag(requiredFlagId) == requiredFlagValue);
        }
    }

    [Serializable]
    public class DialogueNode
    {
        public string id;
        public string speaker;
        [TextArea] public string text;
        public string importantFragment;
        public string nextNodeId;
        public List<DialogueChoice> choices = new List<DialogueChoice>();

        public DialogueNode Clone()
        {
            DialogueNode copy = new DialogueNode
            {
                id = id,
                speaker = speaker,
                text = text,
                importantFragment = importantFragment,
                nextNodeId = nextNodeId,
                choices = new List<DialogueChoice>()
            };
            if (choices != null)
            {
                foreach (DialogueChoice choice in choices)
                {
                    if (choice == null) continue;
                    copy.choices.Add(new DialogueChoice
                    {
                        text = choice.text,
                        targetNodeId = choice.targetNodeId,
                        requiredFlagId = choice.requiredFlagId,
                        requiredFlagValue = choice.requiredFlagValue
                    });
                }
            }
            return copy;
        }
    }

    private readonly List<DialogueNode> graph = new List<DialogueNode>();
    private InteractableObject conversationTarget;
    private string completionFlagId;

    public event Action Closed;
    public bool IsActive { get; private set; }
    public bool IsKeyDialogue { get; private set; }
    public DialogueNode CurrentNode { get; private set; }
    public InteractableObject ConversationTarget { get { return conversationTarget; } }

    public IList<DialogueChoice> AvailableChoices
    {
        get
        {
            List<DialogueChoice> result = new List<DialogueChoice>();
            if (CurrentNode == null || GameSession.Instance == null) return result;
            if (CurrentNode.choices == null) return result;
            foreach (DialogueChoice choice in CurrentNode.choices)
                if (choice != null && !string.IsNullOrEmpty(choice.targetNodeId) &&
                    FindNode(choice.targetNodeId) != null && choice.IsAvailable(GameSession.Instance.Data))
                    result.Add(choice);
            return result.AsReadOnly();
        }
    }

    public void Begin(InteractableObject target)
    {
        if (target == null) return;
        Begin(target, CreatePrototypeGraph(target));
    }

    /// <summary>Starts a conversation from caller-provided nodes; the input list is copied.</summary>
    public void Begin(InteractableObject target, IEnumerable<DialogueNode> nodes, string startNodeId = null)
    {
        if (target == null) return;
        if (IsActive) Cancel();

        conversationTarget = target;
        IsKeyDialogue = target.Kind == InteractableKind.KeyNpc;
        completionFlagId = target.CompletionFlagId;

        graph.Clear();
        HashSet<string> ids = new HashSet<string>();
        if (nodes != null)
        {
            foreach (DialogueNode node in nodes)
            {
                if (node == null || string.IsNullOrEmpty(node.id) || !ids.Add(node.id)) continue;
                graph.Add(node.Clone());
            }
        }

        IsActive = graph.Count > 0;
        string entryNodeId = string.IsNullOrEmpty(startNodeId) && IsActive ? graph[0].id : startNodeId;
        CurrentNode = IsActive && !string.IsNullOrEmpty(entryNodeId) ? FindNode(entryNodeId) : null;
        IsActive = CurrentNode != null;
        if (!IsActive) Finish(false);
    }

    public void Advance()
    {
        if (!IsActive || CurrentNode == null || AvailableChoices.Count > 0) return;
        if (string.IsNullOrEmpty(CurrentNode.nextNodeId))
        {
            Finish(true);
            return;
        }
        CurrentNode = FindNode(CurrentNode.nextNodeId);
        if (CurrentNode == null) Finish(true);
    }

    public void Choose(DialogueChoice choice)
    {
        if (!IsActive || choice == null || !AvailableChoices.Contains(choice)) return;
        DialogueNode nextNode = FindNode(choice.targetNodeId);
        if (nextNode != null) CurrentNode = nextNode;
    }

    public bool RecordCurrentFragment(JournalSystem journalSystem)
    {
        if (!IsActive || !IsKeyDialogue || CurrentNode == null ||
            string.IsNullOrEmpty(CurrentNode.importantFragment) || journalSystem == null)
            return false;

        string location = GameSession.Instance.Data.CurrentLocation;
        string id = "dialogue:" + location + ":" + conversationTarget.Id + ":" + CurrentNode.id;
        return journalSystem.AddEntry(id, "Фрагмент разговора", CurrentNode.importantFragment,
            CurrentNode.speaker, "Свидетельство");
    }

    public Sprite GetCurrentPortrait()
    {
        if (!IsKeyDialogue || CurrentNode == null) return null;
        if (CurrentNode.speaker == "Вы")
        {
            SpriteRenderer playerRenderer = GetComponent<SpriteRenderer>();
            return playerRenderer != null ? playerRenderer.sprite : null;
        }
        SpriteRenderer npcRenderer = conversationTarget != null ? conversationTarget.GetComponent<SpriteRenderer>() : null;
        return npcRenderer != null ? npcRenderer.sprite : null;
    }

    public void Cancel()
    {
        if (IsActive) Finish(false);
    }

    private List<DialogueNode> CreatePrototypeGraph(InteractableObject target)
    {
        List<DialogueNode> nodes = new List<DialogueNode>();
        if (target.Kind == InteractableKind.KeyNpc)
        {
            bool archive = target.gameObject.scene.name == "ArchiveScene";
            string npcName = archive ? "Архивариус" : "Марта";
            nodes.Add(CreateNode("question_1", "Вы", "Вы были здесь вчера вечером?", null, "answer_1"));
            nodes.Add(CreateNode("answer_1", npcName,
                archive ? "Я закрывал архив после девяти." : "Я закрывала комнату после девяти.",
                archive ? "Архив закрыли после девяти." : "После девяти я закрыла комнату.", "question_2"));
            nodes.Add(CreateNode("question_2", "Вы", "Кто-нибудь заходил после этого?", null, "answer_2"));
            nodes.Add(CreateNode("answer_2", npcName,
                archive ? "Я слышал шаги между стеллажами, но никого не видел." : "Слышала шаги у восточной двери, но никого не видела.",
                archive ? "Шаги слышали между стеллажами." : "Слышала шаги у восточной двери.", null));
        }
        else
        {
            nodes.Add(CreateNode("secondary_1", string.Empty, "Я ничего не знаю. Просто проходил мимо.", null, "secondary_2"));
            nodes.Add(CreateNode("secondary_2", string.Empty, "Кажется, здесь кто-то оставил эту вещь.", null, null));
        }
        return nodes;
    }

    private static DialogueNode CreateNode(string id, string speaker, string text, string fragment, string nextId)
    {
        return new DialogueNode
        {
            id = id,
            speaker = speaker,
            text = text,
            importantFragment = fragment,
            nextNodeId = nextId
        };
    }

    private DialogueNode FindNode(string id)
    {
        return graph.Find(node => node.id == id);
    }

    private void Finish(bool completed)
    {
        if (completed && !string.IsNullOrEmpty(completionFlagId) && GameSession.Instance != null)
            GameSession.Instance.Data.SetFlag(completionFlagId, true);
        IsActive = false;
        CurrentNode = null;
        conversationTarget = null;
        Action handler = Closed;
        if (handler != null) handler();
    }
}
