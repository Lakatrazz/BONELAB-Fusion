using Il2CppSLZ.Rig;
using Il2CppSLZ.VRMK;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Marrow.Integration;
using LabFusion.MarrowIntegration;
using LabFusion.SDK.Points;

using UnityEngine;

using Avatar = Il2CppSLZ.VRMK.Avatar;

using Il2Action = Il2CppSystem.Action;
using Il2ActionBool = Il2CppSystem.Action<bool>;
using Il2Delegate = Il2CppSystem.Delegate;

namespace LabFusion.SDK.Cosmetics;

public class CosmeticInstance
{
    public RigManager rigManager;
    public Avatar avatar;

    public GameObject accessory;
    public Transform transform;

    public bool isHiddenInView;

    public RigPoint itemPoint;

    public FusionDictionary<Mirror, GameObject> mirrors = new(new UnityComparer());
    public FusionDictionary<RigPoint, MarrowCosmeticPoint> points = new();

    private bool _destroyed = false;
    public bool IsDestroyed => _destroyed;

    public CosmeticInstance(PointItemPayload payload, GameObject accessory, bool isHiddenInView, RigPoint itemPoint)
    {
        rigManager = payload.rigManager;
        avatar = null;

        this.accessory = accessory;
        transform = accessory.transform;

        this.isHiddenInView = isHiddenInView;

        this.itemPoint = itemPoint;

        UpdateVisibility(false);

        Hook();
    }

    private void Hook()
    {
        // We want our code to execute first in the RigManager, before the head is overriden
        // So we combine these two delegates manually
        rigManager.OnPostLateUpdate = Il2Delegate.Combine((Il2Action)OnPostLateUpdate, rigManager.OnPostLateUpdate).Cast<Il2Action>();

        OpenControllerRig.OnPauseStateChange += (Il2ActionBool)OnPauseStateChange;
    }

    private void Unhook()
    {
        if (!rigManager.IsNOC())
        {
            rigManager.OnPostLateUpdate -= (Il2Action)OnPostLateUpdate;
            OpenControllerRig.OnPauseStateChange -= (Il2ActionBool)OnPauseStateChange;
        }
    }

    private void OnPostLateUpdate()
    {
        if (IsDestroyed)
            return;

        Update(itemPoint);

        UpdateMirrors();
    }

    private void OnPauseStateChange(bool value)
    {
        if (IsDestroyed)
            return;

        UpdateVisibility(value);
    }

    private void UpdateAvatar(Avatar avatar)
    {
        this.avatar = avatar;
        points = new();

        //foreach (var component in avatar.GetComponentsInChildren<MarrowCosmeticPoint>())
        //{
        //    var casted = component.TryCast<MarrowCosmeticPoint>();
        //
        //    if (!points.ContainsKey(casted.Point))
        //    {
        //        points.Add(casted.Point, casted);
        //    }
        //}
    }

    private void UpdateVisibility(bool paused)
    {
        if (!paused && isHiddenInView)
            accessory.SetActive(false);
        else
            accessory.SetActive(true);
    }

    public void Update(RigPoint itemPoint)
    {
        // Compare avatar
        if (rigManager.avatar != avatar)
        {
            UpdateAvatar(rigManager.avatar);
        }

        // Get item transform
        Vector3 position;
        Quaternion rotation;
        Vector3 scale;
        // SDK offset transform
        if (points.TryGetValue(itemPoint, out var component))
        {
            AccessoryItemHelper.GetTransform(component, out position, out rotation, out scale);
        }
        // Auto calculated transform
        else
        {
            AccessoryItemHelper.GetTransform(itemPoint, rigManager, out position, out rotation, out scale);
        }

        transform.SetPositionAndRotation(position, rotation);
        transform.localScale = scale;
    }

    public void InsertMirror(Mirror mirror, GameObject accessory)
    {
        if (mirrors.ContainsKey(mirror))
            return;

        mirrors.Add(mirror, accessory);
    }

    public void RemoveMirror(Mirror mirror)
    {
        if (mirrors.TryGetValue(mirror, out var accessory))
        {
            mirrors.Remove(mirror);

            GameObject.Destroy(accessory);
        }
    }

    public void UpdateMirrors()
    {
        List<Mirror> mirrorsToRemove = null;

        // Update all mirrors
        foreach (var mirror in mirrors)
        {
            if (mirror.Key && mirror.Value)
            {
                // Remove the mirror if its disabled
                if (!mirror.Key.isActiveAndEnabled)
                {
                    mirrorsToRemove ??= new List<Mirror>();

                    mirrorsToRemove.Add(mirror.Key);
                    continue;
                }

                // Set the mirror accessory active or inactive
                if (mirror.Key.rigManager != rigManager)
                {
                    mirror.Value.SetActive(false);
                }
                else
                {
                    mirror.Value.SetActive(true);

                    Transform reflectTran = mirror.Key._reflectTran;

                    Transform accessory = mirror.Value.transform;

                    // Get rotation
                    Vector3 forward = transform.forward;
                    Vector3 up = transform.up;

                    Vector3 reflectLine = reflectTran.forward;

                    forward = Vector3.Reflect(forward, reflectLine);
                    up = Vector3.Reflect(up, reflectLine);

                    Quaternion rotation = Quaternion.LookRotation(forward, up);

                    // Get position
                    Vector3 position = transform.position - reflectTran.position;
                    position = Vector3.Reflect(position, reflectTran.forward);
                    position += reflectTran.position;

                    // Set position, rotation, and scale
                    accessory.SetPositionAndRotation(position, rotation);
                    accessory.localScale = transform.localScale;
                }
            }
        }

        // Remove necessary mirrors
        if (mirrorsToRemove != null)
        {
            foreach (var mirror in mirrorsToRemove)
                RemoveMirror(mirror);
        }
    }

    public bool IsValid()
    {
        return !rigManager.IsNOC() && !accessory.IsNOC();
    }

    public void Cleanup()
    {
        _destroyed = true;

        if (!accessory.IsNOC())
            GameObject.Destroy(accessory);

        foreach (var mirror in mirrors)
        {
            if (!mirror.Value.IsNOC())
                GameObject.Destroy(mirror.Value);
        }

        mirrors.Clear();

        Unhook();
    }
}