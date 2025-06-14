namespace LabFusion.UI.Popups;

/// <summary>
/// All text, icons, and events relating to a popup or menu notification.
/// </summary>
public class Notification
{
    // Text settings
    /// <summary>
    /// The title of the notification.
    /// </summary>
    public NotificationText Title { get; set; } = new NotificationText("New Notification");

    /// <summary>
    /// The main body of the notification.
    /// </summary>
    public NotificationText Message { get; set; }

    /// <summary>
    /// An identifying tag for the notification. Used for cancelling specific notifications when they are not needed.
    /// </summary>
    public string Tag { get; set; } = null;

    // Popup settings
    /// <summary>
    /// Should this notification popup?
    /// </summary>
    public bool ShowPopup { get; set; } = true;

    /// <summary>
    /// How long the notification will be up.
    /// </summary>
    public float PopupLength { get; set; } = 2f;

    /// <summary>
    /// The type of notification this is. Changes the icon.
    /// </summary>
    public NotificationType Type { get; set; } = NotificationType.INFORMATION;

    // BoneMenu settings
    /// <summary>
    /// Will the notification popup inside of the menu tab?
    /// </summary>
    public bool SaveToMenu { get; set; } = true;

    /// <summary>
    /// Invoked when the notification is accepted. Requires <see cref="SaveToMenu"/> to be true.
    /// </summary>
    public Action OnAccepted { get; set; } = null;

    /// <summary>
    /// Invoked when the notification is declined. Requires <see cref="SaveToMenu"/> to be true.
    /// </summary>
    public Action OnDeclined { get; set; } = null;
}
