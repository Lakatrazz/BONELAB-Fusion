using LabFusion.UI.Popups;

namespace LabFusion.Network;

public static class NetworkLayerNotifications
{   
    public const string NotificationTag = "NetworkLayer";

    public static void SendLoggingInNotification()
    {
        Notifier.Cancel(NotificationTag);

        Notifier.Send(new Notification()
        {
            Title = "Please Wait!",
            Message = $"Logging in to {NetworkLayerDeterminer.LoadedLayer.Platform}...",
            Tag = NotificationTag,
            SaveToMenu = false,
            ShowPopup = true,
            Type = NotificationType.INFORMATION,
        });
    }

    public static void SendLoginFailedNotification()
    {
        Notifier.Cancel(NotificationTag);

        Notifier.Send(new Notification()
        {
            Title = "Please try again!",
            Message = $"Failed to login to {NetworkLayerDeterminer.LoadedLayer.Platform}!",
            Tag = NotificationTag,
            SaveToMenu = false,
            ShowPopup = true,
            Type = NotificationType.ERROR,
        });
    }
}