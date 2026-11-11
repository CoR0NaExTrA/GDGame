using UnityEngine;

public class LocationMapUI
{
    public void Draw(LocationSystem location, TimeSystem time)
    {
        if (location == null) return;
        float width = Mathf.Min(560, Screen.width - 48);
        Rect panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - 280) * 0.5f, width, 280);
        GUI.Box(panel, GUIContent.none, UITheme.Panel);
        GUI.Label(new Rect(panel.x + 28, panel.y + 24, width - 56, 36), "Выберите локацию", UITheme.Heading);
        GUI.Label(new Rect(panel.x + 28, panel.y + 64, width - 56, 28), "Переход занимает 20 энергии.", UITheme.Muted);

        System.Collections.Generic.IList<LocationSystem.LocationOption> options = location.Locations;
        for (int i = 0; i < options.Count; i++)
        {
            LocationSystem.LocationOption option = options[i];
            if (GUI.Button(new Rect(panel.x + 28, panel.y + 108 + i * 56, width - 56, 48), option.displayName, UITheme.Button))
                location.TravelTo(option.sceneName, time);
        }
        GUI.Label(new Rect(panel.x + 28, panel.y + 232, width - 56, 28), "M — закрыть карту", UITheme.Muted);
    }
}
