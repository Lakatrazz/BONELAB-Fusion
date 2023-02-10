using BoneLib.BoneMenu.Elements;

using LabFusion.BoneMenu;
using LabFusion.Data;
using LabFusion.Extensions;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace LabFusion.Utilities
{
    internal static class FusionNotifier
    {
        public struct Notification {
            public string title;
            public Color titleColor;

            public string message;
            public Color messageColor;

            public bool isMenuItem;

            public bool isPopup;
            public float popupLength;

            public Action<MenuCategory> onCreateCategory;

            public Notification(string title, Color titleColor, string message, Color messageColor, bool isMenuItem, bool isPopup, float popupLength, Action<MenuCategory> onCreateCategory) {
                this.title = title;
                this.titleColor = titleColor;

                this.message = message;
                this.messageColor = messageColor;

                this.isMenuItem = isMenuItem;

                this.isPopup = isPopup;
                this.popupLength = popupLength;

                this.onCreateCategory = onCreateCategory;
            }
        }

        public const float DefaultDuration = 2f;

        private static readonly Queue<Notification> _queuedNotifications = new Queue<Notification>();
        private static ulong _notificationNumber = 0;

        private static bool _hasEnabledTutorialRig = false;

        public static void Send(string title, string message) {
            Send(title, message, true, true);
        }

        public static void Send(string title, string message, bool isMenuItem, bool isPopup) {
            Send(title, Color.white, message, Color.white, isMenuItem, isPopup, DefaultDuration, null);
        }

        public static void Send(string title, Color titleColor, string message, Color messageColor, bool isMenuItem, bool isPopup, float popupLength, Action<MenuCategory> onCreateCategory) {
            var notification = new Notification(
                title,
                titleColor,
                message,
                messageColor,
                isMenuItem,
                isPopup,
                popupLength,
                onCreateCategory
                );

            QueueNotification(notification);
        }

        private static void QueueNotification(Notification notification) { 
            _queuedNotifications.Enqueue(notification);
        }

        private static void DequeueNotification() {
            var notification = _queuedNotifications.Dequeue();

            // Add to bonemenu
            if (notification.isMenuItem) {
                // Use a name generated with an index because BoneMenu returns an existing category if names match
                string generated = $"Internal_Notification_Generated_{_notificationNumber}";
                var category = BoneMenuCreator.NotificationCategory.CreateCategory(generated, notification.titleColor);

                category.SetName(notification.title);

                _notificationNumber++;

                if (!string.IsNullOrWhiteSpace(notification.message))
                    category.CreateFunctionElement(notification.message, notification.messageColor, null);

                category.CreateFunctionElement("Mark as Read", Color.red, () =>
                {
                    BoneMenuCreator.RemoveNotification(category);
                });

                notification.onCreateCategory.InvokeSafe(category, "executing Notification.OnCreateCategory");
            }

            // Show to the player
            var rm = RigData.RigReferences.RigManager;

            if (notification.isPopup && !rm.IsNOC()) {
                var tutorialRig = rm.tutorialRig;
                var headTitles = tutorialRig.headTitles;

                EnableTutorialRig();

                string incomingTitle = "New Notification";
                string incomingSubTitle = notification.message;
                float holdTime = notification.popupLength;

                headTitles.CUSTOMDISPLAY(incomingTitle, incomingSubTitle, null, holdTime);
            }
        }

        internal static void EnableTutorialRig() {
            var rm = RigData.RigReferences.RigManager;

            if (!rm.IsNOC()) {
                var tutorialRig = rm.tutorialRig;
                var headTitles = tutorialRig.headTitles;

                // Make sure the tutorial rig/head titles are enabled
                tutorialRig.gameObject.SetActive(true);
                headTitles.gameObject.SetActive(true);
            }
        }

        internal static void OnUpdate() {
            // Make sure we aren't loading so we can dequeue existing notifications
            if (_queuedNotifications.Count > 0 && !LevelWarehouseUtilities.IsLoading()) {
                // Enable the tutorial rig a frame before showing notifs
                if (!_hasEnabledTutorialRig) {
                    EnableTutorialRig();
                    _hasEnabledTutorialRig = true;
                }
                else {
                    // Dequeue notifications
                    int count = _queuedNotifications.Count;
                    for (var i = 0; i < count; i++) {
                        DequeueNotification();
                    }
                }
            }
            else
                _hasEnabledTutorialRig = false;
        }
    }
}
