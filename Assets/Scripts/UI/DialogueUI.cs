using UnityEngine;

public class DialogueUI
{
    public void Draw(DialogueSystem dialogue, JournalSystem journal)
    {
        DialogueSystem.DialogueNode node = dialogue != null ? dialogue.CurrentNode : null;
        if (node == null) return;
        float width = Mathf.Min(760, Screen.width - 48);
        Rect panel = new Rect((Screen.width - width) * 0.5f, Screen.height - 245, width, 205);
        GUI.Box(panel, GUIContent.none, UITheme.Panel);

        if (dialogue.IsKeyDialogue)
        {
            Rect portrait = new Rect(panel.x + 20, panel.y + 22, 56, 56);
            GUI.Box(portrait, GUIContent.none, UITheme.Button);
            Sprite sprite = dialogue.GetCurrentPortrait();
            if (sprite != null)
            {
                Rect pixels = sprite.textureRect;
                Rect uv = new Rect(pixels.x / sprite.texture.width, pixels.y / sprite.texture.height,
                    pixels.width / sprite.texture.width, pixels.height / sprite.texture.height);
                GUI.DrawTextureWithTexCoords(portrait, sprite.texture, uv, true);
            }
            GUI.Label(new Rect(panel.x + 90, panel.y + 22, width - 120, 30), node.speaker, UITheme.Heading);
            GUI.Label(new Rect(panel.x + 90, panel.y + 58, width - 120, 62), node.text, UITheme.Body);
            if (!string.IsNullOrEmpty(node.importantFragment))
            {
                GUI.Label(new Rect(panel.x + 24, panel.y + 127, 118, 25), "Важный фрагмент", UITheme.Muted);
                GUI.Label(new Rect(panel.x + 155, panel.y + 127, width - 330, 30), "«" + node.importantFragment + "»", UITheme.Body);
                if (GUI.Button(new Rect(panel.x + width - 165, panel.y + 124, 140, 34), "Записать", UITheme.Button))
                    dialogue.RecordCurrentFragment(journal);
            }
        }
        else
        {
            GUI.Label(new Rect(panel.x + 24, panel.y + 24, width - 48, 24), node.text, UITheme.Body);
        }

        System.Collections.Generic.IList<DialogueSystem.DialogueChoice> choices = dialogue.AvailableChoices;
        if (choices.Count > 0)
        {
            float buttonWidth = (width - 56) / choices.Count;
            for (int i = 0; i < choices.Count; i++)
            {
                DialogueSystem.DialogueChoice choice = choices[i];
                if (GUI.Button(new Rect(panel.x + 24 + i * (buttonWidth + 8), panel.y + 158, buttonWidth, 34), choice.text, UITheme.Button))
                    dialogue.Choose(choice);
            }
        }
        else
        {
            GUI.Label(new Rect(panel.x + 24, panel.y + 169, width - 48, 25), "E / Пробел — дальше", UITheme.Muted);
        }
    }
}
