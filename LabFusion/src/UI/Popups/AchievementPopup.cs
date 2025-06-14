using LabFusion.Data;
using LabFusion.SDK.Achievements;
using LabFusion.Marrow;
using LabFusion.Scene;
using LabFusion.Utilities;
using LabFusion.Marrow.Pool;

using UnityEngine;
using UnityEngine.UI;

using Il2CppTMPro;

using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;

namespace LabFusion.UI.Popups;

public static class AchievementPopup
{
    public const float DefaultDuration = 5f;
    public static readonly Vector3 LocalPosition = new(0f, -0.06f, 0.9f);

    private static readonly Queue<Achievement> _queuedAchievements = new();

    private static float _timeOfPopup = 0f;

    public static void Send(Achievement achievement)
    {
        QueueAchievement(achievement);
    }

    private static void QueueAchievement(Achievement achievement)
    {
        _queuedAchievements.Enqueue(achievement);
    }

    private static void DequeueAchievement()
    {
        _timeOfPopup = TimeUtilities.TimeSinceStartup;
        var achievement = _queuedAchievements.Dequeue();

        var camera = RigData.Refs.ControllerRig.m_head;

        var spawnable = LocalAssetSpawner.CreateSpawnable(FusionSpawnableReferences.AchievementPopupReference);

        LocalAssetSpawner.Register(spawnable);

        LocalAssetSpawner.Spawn(spawnable, Vector3.zero, Quaternion.identity, (poolee) =>
        {
            var popupTransform = poolee.transform;
            popupTransform.parent = camera;

            UIMachineUtilities.OverrideFonts(popupTransform.transform);

            popupTransform.localPosition = LocalPosition;
            popupTransform.localRotation = Quaternion.identity;

            Transform canvas = popupTransform.Find("Offset/Canvas");

            if (achievement.Logo != null)
            {
                canvas.Find("icon").GetComponent<RawImage>().texture = achievement.Logo;
            }

            canvas.Find("title").GetComponent<TMP_Text>().text = achievement.Title;
            canvas.Find("description").GetComponent<TMP_Text>().text = achievement.Description;

            LocalAudioPlayer.Play2dOneShot(new AudioReference(FusionMonoDiscReferences.UITurnOnReference), LocalAudioPlayer.InHeadSettings);

            PooleeHelper.DespawnDelayed(poolee, DefaultDuration + 0.1f);
        });
    }

    internal static bool IsPlayingPopup()
    {
        return TimeUtilities.TimeSinceStartup - _timeOfPopup <= (DefaultDuration + 0.1f);
    }

    internal static void OnInitializeMelon()
    {
        Achievement.OnAchievementCompleted += Send;
    }

    internal static void OnUpdate()
    {
        // Make sure we aren't loading so we can dequeue existing achievements
        if (_queuedAchievements.Count > 0 && FusionSceneManager.HasTargetLoaded() && !FusionSceneManager.IsDelayedLoading() && RigData.HasPlayer)
        {
            // Dequeue achievements
            if (!IsPlayingPopup())
            {
                DequeueAchievement();
            }
        }
    }
}