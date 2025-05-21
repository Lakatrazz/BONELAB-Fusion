using Il2CppTMPro;

using LabFusion.Marrow.Proxies;
using LabFusion.Utilities;
using LabFusion.UI.Popups;

using UnityEngine;

namespace LabFusion.Menu;

public static class MenuNotifications
{
    public static TMP_Text NotificationCountText { get; private set; } = null;

    public static PageElement NotificationPageElement { get; private set; } = null;

    public static List<Notification> SavedNotifications { get; private set; } = new();

    public static void PopulateNotifications(GameObject notificationsPage)
    {
        var topLayout = notificationsPage.transform.Find("layout_Top");

        NotificationCountText = topLayout.Find("text_NotificationCount").GetComponent<TMP_Text>();

        topLayout.Find("button_Clear").GetComponent<FunctionElement>().Do(ClearNotifications);

        NotificationPageElement = notificationsPage.transform.Find("scrollRect_Notifications/Viewport/Content").GetComponent<PageElement>().AddPage();

        RecreateNotifications();

        UpdateCount();
    }

    private static void UpdateCount()
    {
        if (NotificationCountText == null)
        {
            return;
        }

        int count = SavedNotifications.Count;

        if (count <= 0)
        {
            NotificationCountText.text = "You have no notifications.";
        }
        else
        {
            NotificationCountText.text = $"You have {count} notification{(count == 1 ? string.Empty : "s")}.";
        }
    }

    public static void ClearNotifications()
    {
        if (NotificationPageElement == null)
        {
            return;
        }

        SavedNotifications.Clear();

        NotificationPageElement.RemoveElements();

        UpdateCount();
    }

    private static void RecreateNotifications()
    {
        foreach (var notification in SavedNotifications)
        {
            CreateNotification(notification);
        }
    }

    public static void AddNotification(Notification notification)
    {
        SavedNotifications.Insert(0, notification);

        CreateNotification(notification);
    }

    private static void CreateNotification(Notification notification)
    {
        if (NotificationPageElement == null)
        {
            return;
        }

        var notificationElement = NotificationPageElement.AddElement<NotificationElement>(notification.Title.Text);

        notificationElement.GetReferences();

        notificationElement.TitleText.text = notification.Title.Text;
        notificationElement.MessageText.text = notification.Message.Text;

        notificationElement.OnAccepted += OnAccepted;
        notificationElement.OnDeclined += OnDeclined;

        bool hasAction = notification.OnAccepted != null || notification.OnDeclined != null;

        // If this notification doesn't have an action, there's nothing to decline
        if (!hasAction)
        {
            notificationElement.DeclineButton.SetActive(false);
        }

        UpdateCount();

        void OnAccepted()
        {
            notification.OnAccepted?.InvokeSafe("executing FusionNotification.OnAccepted");

            RemoveNotification();
        }

        void OnDeclined()
        {
            notification.OnDeclined?.InvokeSafe("executing FusionNotification.OnDeclined");

            RemoveNotification();
        }

        void RemoveNotification()
        {
            NotificationPageElement.RemoveElement(notificationElement);

            SavedNotifications.Remove(notification);

            UpdateCount();
        }
    }
}
