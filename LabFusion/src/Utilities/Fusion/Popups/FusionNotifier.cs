﻿using Il2CppSLZ.Bonelab;

using LabFusion.BoneMenu;
using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Scene;

using UnityEngine;

using Page = BoneLib.BoneMenu.Page;

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
    public string text;

    /// <summary>
    /// The color of the text.
    /// </summary>
    public Color color;

    /// <summary>
    /// Should rich text be allowed?
    /// </summary>
    public bool richText;

    public NotificationText(string text) : this(text, Color.white) { }

    public NotificationText(string text, Color color, bool richText = false)
    {
        if (!richText)
        {
            text = text.RemoveRichText();
        }

        this.text = text;
        this.color = color;
        this.richText = richText;
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
    public NotificationText title;

    /// <summary>
    /// The main body of the notification.
    /// </summary>
    public NotificationText message;

    // Popup settings
    /// <summary>
    /// Should the title be used on the popup? (If false, it shows "New Notification".)
    /// </summary>
    public bool showTitleOnPopup = false;

    /// <summary>
    /// Should this notification popup?
    /// </summary>
    public bool isPopup = true;

    /// <summary>
    /// How long the notification will be up.
    /// </summary>
    public float popupLength = 2f;

    /// <summary>
    /// The type of notification this is. Changes the icon.
    /// </summary>
    public NotificationType type = NotificationType.INFORMATION;

    // BoneMenu settings
    /// <summary>
    /// Will the notification popup inside of the menu tab?
    /// </summary>
    public bool isMenuItem = true;

    /// <summary>
    /// A hook for adding custom functions in the menu. Requires <see cref="isMenuItem"/> to be on.
    /// </summary>
    public Action<Page> onCreateCategory = null;
}

public static class FusionNotifier
{
    public const float DefaultDuration = 2f;

    public const float DefaultScaleTime = 0.4f;

    private static readonly Queue<FusionNotification> _queuedNotifications = new();
    private static ulong _notificationNumber = 0;

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

        // Add to bonemenu
        if (notification.isMenuItem)
        {
            // Use a name generated with an index because BoneMenu returns an existing category if names match
            string generated = $"Internal_Notification_Generated_{_notificationNumber}";
            var page = BoneMenuCreator.NotificationCategory.CreatePage(generated, notification.title.color, 0, false);
            var pageLink = BoneMenuCreator.NotificationCategory.CreatePageLink(page);

            page.Name = notification.title.text;

            _notificationNumber++;

            if (!string.IsNullOrWhiteSpace(notification.message.text))
                page.CreateFunction(notification.message.text, notification.message.color, null);

            page.CreateFunction("Mark as Read", Color.red, () =>
            {
                BoneMenuCreator.RemoveNotification(pageLink);
            });

            notification.onCreateCategory.InvokeSafe(page, "executing Notification.OnCreateCategory");
        }

        // Show to the player
        var rm = RigData.Refs.RigManager;

        if (notification.isPopup && !rm.IsNOC())
        {
            var tutorialRig = TutorialRig.Instance;
            var headTitles = tutorialRig.headTitles;

            EnableTutorialRig();

            string incomingTitle = "New Notification";

            if (notification.showTitleOnPopup)
                incomingTitle = notification.title.text;

            string incomingSubTitle = notification.message.text;

            Sprite incomingSprite = GetPopupSprite(notification);

            float holdTime = notification.popupLength;

            float timeToScale = Mathf.Lerp(0.05f, DefaultScaleTime, Mathf.Clamp01(holdTime - 1f));

            headTitles.timeToScale = timeToScale;

            headTitles.CUSTOMDISPLAY(incomingTitle, incomingSubTitle, incomingSprite, holdTime);
            headTitles.sr_element.sprite = incomingSprite;
        }
    }

    private static Sprite GetPopupSprite(FusionNotification notification)
    {
        Texture2D incomingTexture = notification.type switch
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
        var rm = RigData.Refs.RigManager;

        if (!rm.IsNOC())
        {
            var tutorialRig = TutorialRig.Instance;
            var headTitles = tutorialRig.headTitles;

            // Make sure the tutorial rig/head titles are enabled
            tutorialRig.gameObject.SetActive(true);
            headTitles.gameObject.SetActive(true);
        }
    }

    internal static bool IsPlayingNotification()
    {
        var rm = RigData.Refs.RigManager;

        if (!rm.IsNOC())
        {
            var tutorialRig = TutorialRig.Instance;
            var headTitles = tutorialRig.headTitles;

            return headTitles.headFollower.gameObject.activeInHierarchy;
        }

        return false;
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