using UnityEngine;

public class HUD
{
    public void Draw(SessionData data, InteractionSystem interaction, LocationSystem location)
    {
        if (data == null) return;
        GUI.Label(new Rect(24, 18, 900, 28), "E — действие   TAB — дневник   M — карта   Ctrl+S — сохранить   Ctrl+L — загрузить   Ctrl+N / Ctrl+R — новая игра", UITheme.Muted);
        string period = data.Period == TimePeriod.Night ? "Ночь" : "День";
        string locationName = location != null ? location.CurrentDisplayName : data.CurrentLocation;
        GUI.Label(new Rect(24, 52, 500, 32), "День " + data.DayNumber + " · " + period + " · " + locationName, UITheme.Heading);
        GUI.Label(new Rect(24, 88, 230, 24), "Энергия: " + data.Energy + " / 100", UITheme.Muted);

        GUI.Box(new Rect(24, 115, 220, 16), GUIContent.none, UITheme.Panel);
        Color oldColor = GUI.color;
        GUI.color = data.Period == TimePeriod.Night ? new Color(0.35f, 0.72f, 0.92f) : new Color(0.91f, 0.68f, 0.3f);
        GUI.Box(new Rect(27, 118, 214f * data.Energy / 100f, 10), GUIContent.none);
        GUI.color = oldColor;

        InteractableObject target = interaction != null ? interaction.CurrentTarget : null;
        if (target != null)
            GUI.Label(new Rect(24, 146, 560, 30), "[E] " + target.DisplayName + "  (−" + target.EnergyCost + " энергии)", UITheme.Heading);
    }
}
