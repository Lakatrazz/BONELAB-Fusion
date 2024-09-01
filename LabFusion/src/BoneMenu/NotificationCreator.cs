using BoneLib.BoneMenu;

using UnityEngine;

namespace LabFusion.BoneMenu
{
    using Menu = BoneLib.BoneMenu.Menu;

    public static partial class BoneMenuCreator
    {
        private static Page _notificationCategory;
        public static Page NotificationCategory => _notificationCategory;

        public static void CreateNotificationsMenu(Page page)
        {
            _notificationCategory = page.CreatePage("Notifications", Color.yellow);
            _notificationCategory.CreateFunction("Clear All", Color.white, ClearNotifications);
        }

        private static void ClearNotifications()
        {
            _notificationCategory.RemoveAll();
            _notificationCategory.CreateFunction("Clear All", Color.white, ClearNotifications);
            Menu.OpenPage(_notificationCategory);
        }

        public static void RemoveNotification(Element element)
        {
            if (_notificationCategory.Elements.Contains(element))
            {
                _notificationCategory.Remove(element);
                Menu.OpenPage(_notificationCategory);
            }
        }
    }
}
