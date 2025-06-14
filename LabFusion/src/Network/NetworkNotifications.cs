using LabFusion.UI.Popups;

namespace LabFusion.Network;

public static class NetworkNotifications
{
    public const string NotificationTag = "Network";

    public static void SendStartedServerNotification()
    {
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
