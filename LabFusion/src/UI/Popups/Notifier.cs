using Il2CppSLZ.Bonelab;

using LabFusion.Data;
using LabFusion.Menu;
using LabFusion.Scene;

using UnityEngine;

namespace LabFusion.UI.Popups;

public static class Notifier
{
    public const float DefaultDuration = 2f;

    public const float DefaultScaleTime = 0.4f;

    private static readonly List<Notification> _queuedNotifications = new();

    private static Notification _currentNotification = null;
    public static Notification CurrentNotification => _currentNotification;

    private static bool _hasEnabledTutorialRig = false;

    public static List<Notification> GetNotificationsByTag(string tag)
    {
        List<Notification> taggedNotifications = new();

        foreach (var queued in _queuedNotifications)
        {
            if (queued.Tag == tag)
            {
                taggedNotifications.Add(queued);
            }
        }

        if (CurrentNotification != null && CurrentNotification.Tag == tag)
        {
            taggedNotifications.Add(CurrentNotification);
        }

        return taggedNotifications;
    }

    public static List<Notification> GetAllNotifications()
    {
        List<Notification> notifications = new();

        notifications.AddRange(_queuedNotifications);

        if (CurrentNotification != null)
        {
            notifications.Add(CurrentNotification);
        }

        return notifications;
    }

    public static void Send(Notification notification)
    {
        QueueNotification(notification);
    }

    public static void Cancel(Notification notification) 
    {
        if (_queuedNotifications.Contains(notification))
        {
            _queuedNotifications.Remove(notification);
        }

        if (CurrentNotification == notification)
        {
            _currentNotification = null;
            StopHeadTitles();
        }
    }

    public static void Cancel(string tag)
    {
        var notifications = GetNotificationsByTag(tag);

        foreach (var notification in notifications) 
        {
            Cancel(notification);
        }
    }

    public static void CancelAll()
    {
        var notifications = GetAllNotifications();

        foreach (var notification in notifications)
        {
            Cancel(notification);
        }
    }

    private static void QueueNotification(Notification notification)
    {
        _queuedNotifications.Add(notification);

        // Add to the menu as soon as its queued
        // This way it will still show up even if cancelled
        if (notification.SaveToMenu)
        {
            MenuNotifications.AddNotification(notification);
        }
    }

    private static void DequeueNotification()
    {
        _currentNotification = _queuedNotifications[0];
        _queuedNotifications.RemoveAt(0);

        // Show to the player
        if (CurrentNotification.ShowPopup && RigData.HasPlayer)
        {
            var tutorialRig = TutorialRig.Instance;
            var headTitles = tutorialRig.headTitles;

            EnableTutorialRig();

            string incomingTitle = CurrentNotification.Title.Text;

            string incomingSubTitle = CurrentNotification.Message.Text;

            Sprite incomingSprite = GetPopupSprite(CurrentNotification) ?? headTitles.defaultElementSprite;

            float holdTime = CurrentNotification.PopupLength;

            float timeToScale = Mathf.Lerp(0.05f, DefaultScaleTime, Mathf.Clamp01(holdTime - 1f));

            headTitles.timeToScale = timeToScale;

            headTitles.CUSTOMDISPLAY(incomingTitle, incomingSubTitle, incomingSprite, holdTime);
            headTitles.sr_element.sprite = incomingSprite;
        }
    }

    private static Sprite GetPopupSprite(Notification notification)
    {
        if (!MenuResources.HasResources)
        {
            return null;
        }

        Texture2D incomingTexture = notification.Type switch
        {
            NotificationType.WARNING => MenuResources.GetNotificationIcon("Warning").TryCast<Texture2D>(),
            NotificationType.ERROR => MenuResources.GetNotificationIcon("Error").TryCast<Texture2D>(),
            NotificationType.SUCCESS => MenuResources.GetNotificationIcon("Success").TryCast<Texture2D>(),
            _ => MenuResources.GetNotificationIcon("Information").TryCast<Texture2D>(),
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

    private static void StopHeadTitles()
    {
        if (!RigData.HasPlayer)
        {
            return;
        }

        var tutorialRig = TutorialRig.Instance;

        if (tutorialRig == null)
        {
            return;
        }

        var headTitles = tutorialRig.headTitles;

        headTitles.CLOSEDISPLAY();
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
            else if (!IsPlayingNotification())
            {
                _currentNotification = null;

                // Dequeue notifications
                if (_queuedNotifications.Count > 0)
                {
                    DequeueNotification();
                }
            }
        }
        else
        {
            _hasEnabledTutorialRig = false;
        }
    }
}