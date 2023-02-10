using BoneLib.BoneMenu;
using BoneLib.BoneMenu.Elements;

using UnityEngine;

namespace LabFusion.BoneMenu
{
    internal static partial class BoneMenuCreator
    {
        private static MenuCategory _notificationCategory;
        public static MenuCategory NotificationCategory => _notificationCategory; 

        public static void CreateNotificationsMenu(MenuCategory category)
        {
            _notificationCategory = category.CreateCategory("Notifications", Color.yellow);
            _notificationCategory.CreateFunctionElement("Clear All", Color.white, ClearNotifications);
        }

        private static void ClearNotifications() {
            _notificationCategory.Elements.Clear();
            _notificationCategory.CreateFunctionElement("Clear All", Color.white, ClearNotifications);
            MenuManager.SelectCategory(_notificationCategory);
        }

        public static void RemoveNotification(MenuElement element) {
            if (_notificationCategory.Elements.Contains(element)) {
                _notificationCategory.Elements.Remove(element);
                MenuManager.SelectCategory(_notificationCategory);
            }
        }
    }
}
