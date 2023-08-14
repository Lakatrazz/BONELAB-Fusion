using LabFusion.Data;
using LabFusion.SDK.Achievements;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LabFusion.Utilities
{
    public static class FusionAchievementPopup
    {
        public const float DefaultDuration = 5f;
        public static readonly Vector3 LocalPosition = new(0f, -0.06f, 0.9f);

        private static Queue<Achievement> _queuedAchievements = new();

        private static float _timeOfPopup = 0f;

        public static void Send(Achievement achievement)
        {
            QueueAchievement(achievement);
        }

        private static void QueueAchievement(Achievement achievement)
        {
            _queuedAchievements.Enqueue(achievement);
        }

        private static void DequeueAchievement() {
            _timeOfPopup = Time.realtimeSinceStartup;
            var achievement = _queuedAchievements.Dequeue();

            var camera = RigData.RigReferences.ControllerRig.m_head;
            GameObject instance = GameObject.Instantiate(FusionContentLoader.AchievementPopupPrefab, camera);
            UIMachineUtilities.OverrideFonts(instance.transform);

            instance.transform.localPosition = LocalPosition;

            Transform canvas = instance.transform.Find("Offset/Canvas");

            if (achievement.PreviewImage != null)
                canvas.Find("icon").GetComponent<RawImage>().texture = achievement.PreviewImage;

            canvas.Find("title").GetComponent<TMP_Text>().text = achievement.Title;
            canvas.Find("description").GetComponent<TMP_Text>().text = achievement.Description;

            FusionAudio.Play2D(FusionContentLoader.UITurnOn, 1f);

            GameObject.Destroy(instance, DefaultDuration + 0.1f);
        }

        internal static bool IsPlayingPopup()
        {
            return Time.realtimeSinceStartup - _timeOfPopup <= (DefaultDuration + 0.1f);
        }

        internal static void OnInitializeMelon() {
            Achievement.OnAchievementCompleted += Send;
        }

        internal static void OnUpdate() {
            // Make sure we aren't loading so we can dequeue existing achievements
            if (_queuedAchievements.Count > 0 && FusionSceneManager.HasTargetLoaded() && !FusionSceneManager.IsDelayedLoading() && RigData.HasPlayer) {
                // Dequeue achievements
                if (!IsPlayingPopup()) {
                    DequeueAchievement();
                }
            }
        }
    }
}
