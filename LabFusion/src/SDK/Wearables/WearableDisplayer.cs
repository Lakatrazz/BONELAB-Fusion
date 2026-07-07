using Il2CppSLZ.Marrow;

using LabFusion.Marrow.Integration;
using LabFusion.Utilities;
using LabFusion.Player;

using UnityEngine;

using Il2Action = Il2CppSystem.Action;
using Il2Delegate = Il2CppSystem.Delegate;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.SDK.Wearables;

public class WearableDisplayer
{
    public PlayerID PlayerID { get; set; } = null;

    public bool IsLocal => PlayerID == null || PlayerID.IsMe;

    public bool HasRig { get; private set; } = false;

    public RigManager RigManager { get; private set; } = null;

    public Dictionary<AvatarAnchor, AvatarPointOverride> AvatarPointOverrides { get; } = new();

    public List<Transform> ReflectionOrigins { get; } = new();

    public int ReflectionCount => ReflectionOrigins.Count;

    public List<WearableItem> Wearables { get; } = new();

    public Dictionary<WearableItem, WearableInstance> WearableToInstanceLookup { get; } = new();

    private bool _isPaused = false;
    public bool IsPaused
    {
        get
        {
            return _isPaused;
        }
        set
        {
            if (_isPaused == value)
            {
                return;
            }

            _isPaused = value;

            ApplyWearableVisibility();
        }
    }

    private bool _isShown = true;
    public bool IsShown
    {
        get
        {
            return _isShown;
        }
        set
        {
            if (_isShown == value)
            {
                return;
            }

            _isShown = value;

            ApplyWearableVisibility();
        }
    }

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

    /// <summary>
    /// Adds a reflection origin transform to the displayer, where reflections of wearables will be shown from.
    /// </summary>
    /// <param name="reflectionOrigin"></param>
    public void AddReflectionOrigin(Transform reflectionOrigin)
    {
        // Don't add multiple of the same origin
        if (ReflectionOrigins.Contains(reflectionOrigin))
        {
            return;
        }

        ReflectionOrigins.Add(reflectionOrigin);

        ApplyWearableReflections();
    }

    /// <summary>
    /// Removes a reflection origin from the displayer.
    /// </summary>
    /// <param name="reflectionOrigin"></param>
    public void RemoveReflectionOrigin(Transform reflectionOrigin)
    {
        bool removed = ReflectionOrigins.Remove(reflectionOrigin);

        if (!removed)
        {
            return;
        }

        ApplyWearableReflections();
    }

    /// <summary>
    /// Removes all reflection origins from the displayer.
    /// </summary>
    public void ClearReflectionOrigins()
    {
        ReflectionOrigins.Clear();

        ApplyWearableReflections();
    }

    public void AddWearable(WearableItem wearable)
    {
        if (Wearables.Contains(wearable))
        {
            return;
        }

        Wearables.Add(wearable);

        var wearableInstance = wearable.CreateInstance();
        ApplyWearableConfiguration(wearableInstance);

        WearableToInstanceLookup[wearable] = wearableInstance;

        wearableInstance.Initialize(IsLocal, PlayerID);

        if (RigManager != null)
        {
            wearableInstance.Spawn(RigManager);
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

    public void RemoveWearables()
    {
        foreach (var instance in WearableToInstanceLookup.Values)
        {
            instance.Destroy();
        }

        Wearables.Clear();
        WearableToInstanceLookup.Clear();
    }

    private void SpawnWearables()
    {
        foreach (var pair in WearableToInstanceLookup)
        {
            var instance = pair.Value;

            instance.Spawn(RigManager);
        }
    }

    private void DespawnWearables()
    {
        foreach (var pair in WearableToInstanceLookup)
        {
            var instance = pair.Value;

            instance.Despawn();
        }
    }

    private void HookRig()
    {
        ClearReflectionOrigins();

        // We want our code to execute first in the RigManager, before the head is offset backwards
        // So we combine these two delegates manually
        RigManager.OnPostLateUpdate = Il2Delegate.Combine((Il2Action)OnPostLateUpdate, RigManager.OnPostLateUpdate).Cast<Il2Action>();

        RigManager.onAvatarSwapped += (Il2Action)OnAvatarSwapped;

        // Call avatar swapped initially to get the starting avatar's values
        OnAvatarSwapped();

        // Spawn all wearables for the rig
        SpawnWearables();
    }

    private void UnhookRig()
    {
        ClearReflectionOrigins();

        DespawnWearables();

        if (RigManager == null)
        {
            return;
        }

        RigManager.OnPostLateUpdate -= (Il2Action)OnPostLateUpdate;
        RigManager.onAvatarSwapped -= (Il2Action)OnAvatarSwapped;
    }

    private void OnPostLateUpdate()
    {
        if (!IsShown || IsPaused)
        {
            return;
        }

        float deltaTime = TimeReferences.DeltaTime;

        UpdateWearables(deltaTime);
    }

    private void UpdateWearables(float deltaTime)
    {
        foreach (var instance in WearableToInstanceLookup.Values)
        {
            UpdateMainTransform(instance);
            UpdateReflectionTransforms(instance);

            instance.Tick(deltaTime);
        }
    }

    private void UpdateMainTransform(WearableInstance instance)
    {
        var anchor = instance.Anchor;

        Vector3 position;
        Quaternion rotation;
        Vector3 scale;

        if (AvatarPointOverrides.TryGetValue(anchor, out var avatarPoint))
        {
            WearableTransformCalculator.GetTransform(avatarPoint, out position, out rotation, out scale);
        }
        else
        {
            WearableTransformCalculator.GetTransform(anchor, RigManager, out position, out rotation, out scale);
        }

        instance.UpdateMain(position, rotation, scale);
    }

    private void UpdateReflectionTransforms(WearableInstance instance)
    {
        for (var i = 0; i < ReflectionOrigins.Count; i++)
        {
            var reflectionOrigin = ReflectionOrigins[i];

            instance.UpdateReflection(reflectionOrigin, i);
        }
    }

    private void ApplyWearableConfiguration(WearableInstance instance)
    {
        ApplyWearableVisibility(instance);
        ApplyWearableReflections(instance);
    }

    private void ApplyWearableVisibility()
    {
        foreach (var instance in WearableToInstanceLookup.Values)
        {
            ApplyWearableVisibility(instance);
        }
    }

    private void ApplyWearableVisibility(WearableInstance instance)
    {
        bool isShown = IsShown;
        bool isPrimaryShown = !instance.HiddenInView || !IsLocal || IsPaused;
        bool isReflectionShown = true;

        instance.IsShown = isShown;
        instance.IsPrimaryShown = isPrimaryShown;
        instance.IsReflectionShown = isReflectionShown;
    }

    private void ApplyWearableReflections()
    {
        foreach (var instance in WearableToInstanceLookup.Values)
        {
            ApplyWearableReflections(instance);
        }
    }

    private void ApplyWearableReflections(WearableInstance instance)
    {
        instance.ReflectionCount = ReflectionCount;
    }

    private void OnAvatarSwapped()
    {
        try
        {
            var avatar = RigManager.avatar;

            PopulateAvatarPointOverrides(avatar);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("executing WearableDisplayer.PopulateAvatarPoints", e);
        }
    }

    private void PopulateAvatarPointOverrides(Avatar avatar)
    {
        AvatarPointOverrides.Clear();

        var points = avatar.GetComponentsInChildren<AvatarPointOverride>();

        foreach (var point in points)
        {
            var anchor = point.GetAnchor();

            AvatarPointOverrides[anchor] = point;
        }
    }
}
