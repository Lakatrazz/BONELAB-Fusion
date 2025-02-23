using Il2CppSLZ.Marrow;

using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow.Integration;
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

    public Dictionary<Mirror, GameObject> mirrors = new(new UnityComparer());

    private Dictionary<RigPoint, AvatarCosmeticPoint> _avatarPoints = new();

    private bool _destroyed = false;
    public bool IsDestroyed => _destroyed;

    private bool _isPaused = false;
    public bool IsPaused
    {
        get
        {
            return _isPaused;
        }
        set
        {
            _isPaused = value;

            OnApplyVisibility();
        }
    }

    private bool _isCulled = false;
    private bool IsCulled
    {
        get
        {
            return _isCulled;
        }
        set
        {
            _isCulled = value;

            OnApplyVisibility();
        }
    }

    public NetworkPlayer NetworkPlayer { get; set; } = null;

    public CosmeticInstance(PointItemPayload payload, GameObject accessory, bool isHiddenInView, RigPoint itemPoint)
    {
        rigManager = payload.rigManager;
        avatar = null;

        this.accessory = accessory;
        transform = accessory.transform;

        this.isHiddenInView = isHiddenInView;

        this.itemPoint = itemPoint;

        if (NetworkPlayerManager.TryGetPlayer(rigManager, out var networkPlayer))
        {
            NetworkPlayer = networkPlayer;
        }

        OnApplyVisibility();

        Hook();
    }

    private void Hook()
    {
        // We want our code to execute first in the RigManager, before the head is overriden
        // So we combine these two delegates manually
        rigManager.OnPostLateUpdate = Il2Delegate.Combine((Il2Action)OnPostLateUpdate, rigManager.OnPostLateUpdate).Cast<Il2Action>();

        OpenControllerRig.OnPauseStateChange += (Il2ActionBool)OnPauseStateChange;

        // Hook into the NetworkPlayer's hiding event
        if (NetworkPlayer != null)
        {
            NetworkPlayer.OnHiddenChanged += OnPlayerHiddenChanged;
            IsCulled = NetworkPlayer.IsHidden;
        }
    }

    private void Unhook()
    {
        if (rigManager != null)
        {
            rigManager.OnPostLateUpdate -= (Il2Action)OnPostLateUpdate;
            OpenControllerRig.OnPauseStateChange -= (Il2ActionBool)OnPauseStateChange;
        }

        // Unhook from the NetworkPlayer's hiding event
        if (NetworkPlayer != null)
        {
            NetworkPlayer.OnHiddenChanged -= OnPlayerHiddenChanged;
        }
    }

    private void OnPostLateUpdate()
    {
        if (IsDestroyed)
        {
            return;
        }

        Update(itemPoint);

        UpdateMirrors();
    }

    private void OnPauseStateChange(bool value)
    {
        if (IsDestroyed)
        {
            return;
        }

        IsPaused = value;
    }

    private void OnPlayerHiddenChanged(bool hidden)
    {
        if (IsDestroyed)
        {
            return;
        }

        IsCulled = hidden;
    }

    private void UpdateAvatar(Avatar avatar)
    {
        this.avatar = avatar;

        // Get all cosmetic points using Fusion SDK
        _avatarPoints = new();

        foreach (var point in avatar.GetComponentsInChildren<AvatarCosmeticPoint>())
        {
            var cosmeticPoint = (RigPoint)point.cosmeticPoint.Get();

            if (!_avatarPoints.ContainsKey(cosmeticPoint))
            {
                _avatarPoints.Add(cosmeticPoint, point);
            }
        }
    }

    private void OnApplyVisibility()
    {
        if (IsCulled)
        {
            accessory.SetActive(false);
            return;
        }

        if (!IsPaused && isHiddenInView)
        {
            accessory.SetActive(false);
            return;
        }

        accessory.SetActive(true);
    }

    public void Update(RigPoint itemPoint)
    {
        // Don't update position if this is culled
        if (IsCulled)
        {
            return;
        }

        // Compare avatar
        if (rigManager.avatar != avatar)
        {
            UpdateAvatar(rigManager.avatar);
        }

        // Get item transform
        Vector3 position;
        Quaternion rotation;
        Vector3 scale;

        // SDK provided transform
        if (_avatarPoints.TryGetValue(itemPoint, out var avatarPoint))
        {
            CosmeticItemHelper.GetTransform(avatarPoint, out position, out rotation, out scale);
        }
        // Auto calculated transform
        else
        {
            CosmeticItemHelper.GetTransform(itemPoint, rigManager, out position, out rotation, out scale);
        }

        transform.SetPositionAndRotation(position, rotation);
        transform.localScale = scale;
    }

    public void InsertMirror(Mirror mirror, GameObject accessory)
    {
        if (mirrors.ContainsKey(mirror))
        {
            return;
        }

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
                if (mirror.Key.rigManager != rigManager || IsCulled)
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
        return rigManager != null && accessory != null;
    }

    public void Cleanup()
    {
        _destroyed = true;

        if (accessory != null)
        {
            GameObject.Destroy(accessory);
        }

        foreach (var mirror in mirrors)
        {
            if (mirror.Value != null)
            {
                GameObject.Destroy(mirror.Value);
            }
        }

        mirrors.Clear();

        Unhook();
    }
}