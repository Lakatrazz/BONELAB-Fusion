using Il2CppSLZ.Bonelab;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Menu;
using LabFusion.Scene;

using UnityEngine;

namespace LabFusion.Utilities;

/// <summary>
/// The basic types of notifications that can be sent.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Used to inform the user.
    /// </summary>
    INFORMATION = 0,

    /// <summary>
    /// Used when the user should be notified of a potential issue.
    /// </summary>
    WARNING = 1,

    /// <summary>
    /// Used when the user or program attempts a task and it fails.
    /// </summary>
    ERROR = 2,

    /// <summary>
    /// Used when the user or program performs a task and it suceeds.
    /// </summary>
    SUCCESS = 3,
}

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

/// <summary>
/// The class used for sending notifications to the player. No constructors, provide your own information.
/// </summary>
public class FusionNotification
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

public static class FusionNotifier
{
    public const float DefaultDuration = 2f;

    public const float DefaultScaleTime = 0.4f;

    private static readonly Queue<FusionNotification> _queuedNotifications = new();

    private static bool _hasEnabledTutorialRig = false;

    public static void Send(FusionNotification notification)
    {
        QueueNotification(notification);
    }

    private static void QueueNotification(FusionNotification notification)
    {
        _queuedNotifications.Enqueue(notification);
    }

    private static void DequeueNotification()
    {
        var notification = _queuedNotifications.Dequeue();

        // Add to the menu
        if (notification.SaveToMenu)
        {
            MenuNotifications.AddNotification(notification);
        }

        // Show to the player
        if (notification.ShowPopup && RigData.HasPlayer)
        {
            var tutorialRig = TutorialRig.Instance;
            var headTitles = tutorialRig.headTitles;

            EnableTutorialRig();

            string incomingTitle = notification.Title.Text;

            string incomingSubTitle = notification.Message.Text;

            Sprite incomingSprite = GetPopupSprite(notification);

            float holdTime = notification.PopupLength;

            float timeToScale = Mathf.Lerp(0.05f, DefaultScaleTime, Mathf.Clamp01(holdTime - 1f));

            headTitles.timeToScale = timeToScale;

            headTitles.CUSTOMDISPLAY(incomingTitle, incomingSubTitle, incomingSprite, holdTime);
            headTitles.sr_element.sprite = incomingSprite;
        }
    }

    private static Sprite GetPopupSprite(FusionNotification notification)
    {
        Texture2D incomingTexture = notification.Type switch
        {
            NotificationType.WARNING => FusionContentLoader.NotificationWarning.Asset,
            NotificationType.ERROR => FusionContentLoader.NotificationError.Asset,
            NotificationType.SUCCESS => FusionContentLoader.NotificationSuccess.Asset,
            _ => FusionContentLoader.NotificationInformation.Asset,
        };

        if (incomingTexture != null)
        {
            return Sprite.Create(incomingTexture, new Rect(0.0f, 0.0f, incomingTexture.width, incomingTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
        else
        {
            return null;
        }
    }

    internal static void EnableTutorialRig()
    {
        if (!RigData.HasPlayer)
        {
            return;
        }

        var tutorialRig = TutorialRig.Instance;
        var headTitles = tutorialRig.headTitles;

        // Make sure the tutorial rig/head titles are enabled
        tutorialRig.gameObject.SetActive(true);
        headTitles.gameObject.SetActive(true);
    }

    internal static bool IsPlayingNotification()
    {
        if (!RigData.HasPlayer)
        {
            return false;
        }

        var tutorialRig = TutorialRig.Instance;
        var headTitles = tutorialRig.headTitles;

        return headTitles.headFollower.gameObject.activeInHierarchy;
    }

    internal static void OnUpdate()
    {
        // Make sure we aren't loading so we can dequeue existing notifications
        if (_queuedNotifications.Count > 0 && !FusionSceneManager.IsLoading() && RigData.HasPlayer)
        {
            // Enable the tutorial rig a frame before showing notifs
            if (!_hasEnabledTutorialRig)
            {
                EnableTutorialRig();
                _hasEnabledTutorialRig = true;
            }
            else
            {
                // Dequeue notifications
                if (_queuedNotifications.Count > 0 && !IsPlayingNotification())
                {
                    DequeueNotification();
                }
            }
        }
        else
            _hasEnabledTutorialRig = false;
    }
}