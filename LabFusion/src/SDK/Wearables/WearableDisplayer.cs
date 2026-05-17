using Il2CppSLZ.Marrow;

using LabFusion.Marrow.Integration;
using LabFusion.Utilities;

using UnityEngine;

using Il2Action = Il2CppSystem.Action;
using Il2Delegate = Il2CppSystem.Delegate;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.SDK.Wearables;

public class WearableDisplayer
{
    public bool HasRig { get; private set; } = false;

    public RigManager RigManager { get; private set; } = null;

    public Dictionary<WearablePoint, AvatarCosmeticPoint> AvatarPoints { get; } = new();

    public List<Transform> ReflectionOrigins { get; } = new();

    public List<WearableItem> Wearables { get; } = new();

    public Dictionary<WearableItem, WearableInstance> WearableToInstanceLookup { get; } = new();

    public bool IsPaused { get; private set; } = false;

    public bool IsHidden { get; private set; } = false;

    public void SetRigManager(RigManager rigManager)
    {
        HasRig = true;
        RigManager = rigManager;

        HookRig();
    }

    public void ClearRigManager()
    {
        UnhookRig();

        HasRig = false;
        RigManager = null;
    }

    public void AddReflectionOrigin(Transform reflectionOrigin)
    {
        ReflectionOrigins.Add(reflectionOrigin);
    }

    public void RemoveReflectionOrigin(Transform reflectionOrigin)
    {
        ReflectionOrigins.Remove(reflectionOrigin);
    }

    public void AddWearable(WearableItem wearable)
    {
        if (Wearables.Contains(wearable))
        {
            return;
        }

        Wearables.Add(wearable);

        var wearableInstance = wearable.CreateInstance();
        WearableToInstanceLookup[wearable] = wearableInstance;

        if (RigManager != null)
        {
            wearableInstance.CreateInstance();
            wearableInstance.CreateReflections(ReflectionOrigins.Count);
        }
    }

    public void RemoveWearable(WearableItem wearable)
    {
        if (!Wearables.Contains(wearable))
        {
            return;
        }

        Wearables.Remove(wearable);

        if (WearableToInstanceLookup.TryGetValue(wearable, out var instance))
        {
            instance.Destroy();
            WearableToInstanceLookup.Remove(wearable);
        }
    }

    public void PauseWearables(bool paused)
    {
        if (IsPaused == paused)
        {
            return;
        }

        IsPaused = paused;
    }

    public void HideWearables(bool hidden)
    {
        if (IsHidden == hidden)
        {
            return;
        }

        IsHidden = true;
    }

    private void CreateWearables()
    {
        foreach (var pair in WearableToInstanceLookup)
        {
            var instance = pair.Value;

            instance.CreateInstance();
            instance.CreateReflections(ReflectionOrigins.Count);
        }
    }

    private void DestroyWearables()
    {
        foreach (var pair in WearableToInstanceLookup)
        {
            var instance = pair.Value;

            instance.Destroy();
        }
    }

    private void HookRig()
    {
        // We want our code to execute first in the RigManager, before the head is offset backwards
        // So we combine these two delegates manually
        RigManager.OnPostLateUpdate = Il2Delegate.Combine((Il2Action)OnPostLateUpdate, RigManager.OnPostLateUpdate).Cast<Il2Action>();

        RigManager.onAvatarSwapped += (Il2Action)OnAvatarSwapped;

        // Call avatar swapped initially to get the starting avatar's values
        OnAvatarSwapped();

        CreateWearables();
    }

    private void UnhookRig()
    {
        DestroyWearables();

        if (RigManager == null)
        {
            return;
        }

        RigManager.OnPostLateUpdate -= (Il2Action)OnPostLateUpdate;
        RigManager.onAvatarSwapped -= (Il2Action)OnAvatarSwapped;
    }

    private void OnPostLateUpdate()
    {
        if (IsHidden || IsPaused)
        {
            return;
        }

        UpdateWearables();
    }

    private void UpdateWearables()
    {
        int reflectionCount = ReflectionOrigins.Count;

        foreach (var pair in WearableToInstanceLookup)
        {
            var instance = pair.Value;
            var point = instance.Point;

            Vector3 position;
            Quaternion rotation;
            Vector3 scale;

            if (AvatarPoints.TryGetValue(instance.Point, out var avatarPoint))
            {
                WearableTransformCalculator.GetTransform(avatarPoint, out position, out rotation, out scale);
            }
            else
            {
                WearableTransformCalculator.GetTransform(point, RigManager, out position, out rotation, out scale);
            }

            instance.UpdateWearable(position, rotation, scale);

            for (var i = 0; i < reflectionCount; i++)
            {
                var reflectionOrigin = ReflectionOrigins[i];

                instance.UpdateReflection(reflectionOrigin, i);
            }
        }
    }

    private void OnAvatarSwapped()
    {
        try
        {
            var avatar = RigManager.avatar;

            PopulateAvatarPoints(avatar);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("executing WearableDisplayer.PopulateAvatarPoints", e);
        }
    }

    private void PopulateAvatarPoints(Avatar avatar)
    {
        AvatarPoints.Clear();

        var points = avatar.GetComponentsInChildren<AvatarCosmeticPoint>();

        foreach (var point in points)
        {
            var rigPoint = (WearablePoint)point.cosmeticPoint.Get();

            AvatarPoints[rigPoint] = point;
        }
    }
}
