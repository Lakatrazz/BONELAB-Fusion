using LabFusion.Preferences.Client;

using LabFusion.UI.Popups;

namespace LabFusion.Network;

/// <summary>
/// Common notifications for network related events.
/// </summary>
public static class NetworkNotifications
{
    public const string NotificationTag = "Network";

    public static void SendStartedServerNotification()
    {
        if (!ClientSettings.Notifications.NotifyServerStarted.Value)
        {
            return;
        }

        Notifier.Cancel(NotificationTag);

        Notifier.Send(new Notification()
        {
            Title = "Started Server",
            Message = "Started a server!",
            Tag = NotificationTag,
            SaveToMenu = false,
            ShowPopup = true,
            Type = NotificationType.SUCCESS,
        });
    }

    public static void SendJoinedServerNotification()
    {
        if (!ClientSettings.Notifications.NotifyServerJoined.Value)
        {
            return;
        }

        Notifier.Cancel(NotificationTag);

        Notifier.Send(new Notification()
        {
            Title = "Joined Server",
            Message = "Joined a server!",
            Tag = NotificationTag,
            SaveToMenu = false,
            ShowPopup = true,
            Type = NotificationType.SUCCESS,
        });
    }

    public static void SendDisconnectedNotification()
    {
        SendDisconnectedNotification(null);
    }

    public static void SendDisconnectedNotification(string reason)
    {
        if (!ClientSettings.Notifications.NotifyServerLeft.Value)
        {
            return;
        }

        Notifier.Cancel(NotificationTag);

        if (string.IsNullOrWhiteSpace(reason))
        {
            Notifier.Send(new Notification()
            {
                Title = "Disconnected from Server",
                Message = "Disconnected from the current server!",
                Tag = NotificationTag,
                SaveToMenu = false,
                ShowPopup = true,
            });
        }
        else
        {
            Notifier.Send(new Notification()
            {
                Title = "Disconnected from Server",
                Message = $"You were disconnected for reason: {reason}",
                Tag = NotificationTag,
                SaveToMenu = true,
                ShowPopup = true,
                PopupLength = 5f,
                Type = NotificationType.WARNING,
            });
        }
    }

    public static void SendPlayerJoinedNotification(string name)
    {
        if (!ClientSettings.Notifications.NotifyPlayerJoined.Value)
        {
            return;
        }

        Notifier.Cancel(NotificationTag);

        Notifier.Send(new Notification()
        {
            Title = $"{name} Joined",
            Message = $"{name} joined the server.",
            Tag = NotificationTag,
            SaveToMenu = false,
            ShowPopup = true,
        });
    }

    public static void SendPlayerLeftNotification(string name)
    {
        if (!ClientSettings.Notifications.NotifyPlayerLeft.Value)
        {
            return;
        }

        Notifier.Cancel(NotificationTag);

        Notifier.Send(new Notification()
        {
            Title = $"{name} Left",
            Message = $"{name} left the server.",
            Tag = NotificationTag,
            SaveToMenu = false,
            ShowPopup = true,
        });
    }
}
