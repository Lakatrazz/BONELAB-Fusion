using BoneLib.BoneMenu.Elements;

using LabFusion.BoneMenu;
using LabFusion.Data;
using LabFusion.Extensions;

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Playables;

namespace LabFusion.Utilities
{
    public struct NotificationText {
        public string text;

        public Color color;

        public bool richText;

        public NotificationText(string text) : this(text, Color.white) { }

        public NotificationText(string text, Color color, bool richText = false) {
            if (!richText) {
                text = text.RemoveRichText();
            }

            this.text = text;
            this.color = color;
            this.richText = richText;
        }

        public static implicit operator NotificationText(string text) {
            return new NotificationText(text);
        }
    }

    public class FusionNotification
    {
        // Text settings
        public NotificationText title;

        public NotificationText message;

        // Popup settings
        public bool showTitleOnPopup = false;

        public bool isPopup = true;

        public float popupLength = 2f;

        // BoneMenu settings
        public bool isMenuItem = true;

        public Action<MenuCategory> onCreateCategory = null;
    }

    public static class FusionNotifier
    {
        public const float DefaultDuration = 2f;

        private static readonly Queue<FusionNotification> _queuedNotifications = new Queue<FusionNotification>();
        private static ulong _notificationNumber = 0;

        private static bool _hasEnabledTutorialRig = false;

        public static void Send(FusionNotification notification) {
            QueueNotification(notification);
        }

        private static void QueueNotification(FusionNotification notification) { 
            _queuedNotifications.Enqueue(notification);
        }

        private static void DequeueNotification() {
            var notification = _queuedNotifications.Dequeue();

            // Add to bonemenu
            if (notification.isMenuItem) {
                // Use a name generated with an index because BoneMenu returns an existing category if names match
                string generated = $"Internal_Notification_Generated_{_notificationNumber}";
                var category = BoneMenuCreator.NotificationCategory.CreateCategory(generated, notification.title.color);

                category.SetName(notification.title.text);

                _notificationNumber++;

                if (!string.IsNullOrWhiteSpace(notification.message.text))
                    category.CreateFunctionElement(notification.message.text, notification.message.color, null);

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

                if (notification.showTitleOnPopup)
                    incomingTitle = notification.title.text;

                string incomingSubTitle = notification.message.text;
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

        internal static bool IsPlayingNotification() {
            var rm = RigData.RigReferences.RigManager;

            if (!rm.IsNOC()) {
                var tutorialRig = rm.tutorialRig;
                var headTitles = tutorialRig.headTitles;

                return headTitles.headFollower.gameObject.activeInHierarchy;
            }

            return false;
        }

        internal static void OnUpdate() {
            // Make sure we aren't loading so we can dequeue existing notifications
            if (_queuedNotifications.Count > 0 && !FusionSceneManager.IsLoading() && RigData.HasPlayer) {
                // Enable the tutorial rig a frame before showing notifs
                if (!_hasEnabledTutorialRig) {
                    EnableTutorialRig();
                    _hasEnabledTutorialRig = true;
                }
                else {
                    // Dequeue notifications
                    if (_queuedNotifications.Count > 0 && !IsPlayingNotification()) {
                        DequeueNotification();
                    }
                }
            }
            else
                _hasEnabledTutorialRig = false;
        }
    }
}
