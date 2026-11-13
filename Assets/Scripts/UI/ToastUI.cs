using UnityEngine;

public class ToastUI
{
    private string message;
    private float visibleUntil;

    public void Show(string value)
    {
        message = value;
        visibleUntil = Time.unscaledTime + 2.5f;
    }

    public void Draw()
    {
        if (string.IsNullOrEmpty(message) || Time.unscaledTime >= visibleUntil) return;
        float width = Mathf.Min(380, Screen.width - 40);
        GUI.Box(new Rect((Screen.width - width) * 0.5f, 20, width, 48), message, UITheme.Toast);
    }
}
