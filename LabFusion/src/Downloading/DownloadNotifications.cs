using LabFusion.Preferences.Client;
using LabFusion.UI.Popups;

namespace LabFusion.Downloading;

public static class DownloadNotifications
{
    public const string NotificationTag = "Download";

    public static void SendDownloadNotification(string palletTitle)
    {
        if (!ClientSettings.Downloading.NotifyDownloads.Value)
        {
            return;
        }

        Notifier.Cancel(NotificationTag);

        Notifier.Send(new()
        {
            ShowPopup = true,
            SaveToMenu = false,
            Title = "Download Completed",
            Tag = NotificationTag,
            Type = NotificationType.SUCCESS,
            PopupLength = 4f,
            Message = $"Finished installing {palletTitle}!"
        });
    }
}
