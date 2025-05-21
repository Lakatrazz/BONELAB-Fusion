using LabFusion.Extensions;

using UnityEngine;

namespace LabFusion.UI.Popups;

/// <summary>
/// The class used to supply text in a notification.
/// </summary>
public struct NotificationText
{
    /// <summary>
    /// The text.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// The color of the text.
    /// </summary>
    public Color Color { get; set; }

    /// <summary>
    /// Should rich text be allowed?
    /// </summary>
    public bool RichText { get; set; }

    public NotificationText(string text) : this(text, Color.white) { }

    public NotificationText(string text, Color color, bool richText = false)
    {
        if (!richText)
        {
            text = text.RemoveRichText();
        }

        this.Text = text;
        this.Color = color;
        this.RichText = richText;
    }

    public static implicit operator NotificationText(string text)
    {
        return new NotificationText(text);
    }
}