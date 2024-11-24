#nullable enable

using Il2CppSLZ.Marrow;
using Il2CppSLZ.VRMK;

using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.SDK.Metadata;
using LabFusion.Utilities;

namespace LabFusion.Player;

public delegate void PlayerGrabDelegate(Hand hand, Grip grip);

public delegate void PlayerAvatarDelegate(Avatar avatar, string barcode);

public static class LocalPlayer
{
    public static PlayerGrabDelegate? OnGrab { get; set; }
    public static PlayerGrabDelegate? OnRelease { get; set; }

    public static Action<RigManager>? OnLocalRigCreated { get; set; }

    public static event PlayerAvatarDelegate? OnAvatarChanged;

    public static bool RagdollOnDeath => NetworkInfo.HasServer;

    private static string _username = "Player";
    public static string Username
    {
        get
        {
            return _username;
        }
        set
        {
            _username = value;

            Metadata.TrySetMetadata(MetadataHelper.UsernameKey, value);

            OnUsernameChanged?.InvokeSafe(value, "executing OnUsernameChanged");
        }
    }

    public static event Action<string>? OnUsernameChanged;

    public static event Action? OnApplyInitialMetadata;

    public static NetworkMetadata Metadata { get; } = new();

    internal static void OnInitializeMelon()
    {
        Metadata.OnTrySetMetadata += OnTrySetMetadata;
        Metadata.OnTryRemoveMetadata += OnTryRemoveMetadata;
    }

    private static bool OnTrySetMetadata(string key, string value)
    {
        Metadata.ForceSetLocalMetadata(key, value);

        var localId = PlayerIdManager.LocalId;

        if (localId != null)
        {
            localId.Metadata.TrySetMetadata(key, value);
        }

        return true;
    }

    private static bool OnTryRemoveMetadata(string key)
    {
        Metadata.ForceRemoveLocalMetadata(key);

        var localId = PlayerIdManager.LocalId;

        if (localId != null)
        {
            localId.Metadata.TryRemoveMetadata(key);
        }

        return true;
    }

    internal static void InvokeAvatarChanged(Avatar avatar, string barcode)
    {
        OnAvatarChanged?.InvokeSafe(avatar, barcode, "executing LocalPlayer.OnAvatarChanged");
    }

    internal static void InvokeApplyInitialMetadata()
    {
        OnApplyInitialMetadata?.InvokeSafe("executing LocalPlayer.OnApplyInitialMetadata");
    }

    /// <summary>
    /// Gets the Local Player's NetworkPlayer.
    /// </summary>
    /// <returns></returns>
    public static NetworkPlayer? GetNetworkPlayer()
    {
        if (!NetworkInfo.HasServer)
        {
            return null;
        }

        if (NetworkPlayerManager.TryGetPlayer(PlayerIdManager.LocalId, out var player))
        {
            return player;
        }

        return null;
    }

    /// <summary>
    /// Removes all constraints from the Local Player.
    /// </summary>
    public static void ClearConstraints()
    {
        if (!RigData.HasPlayer)
        {
            return;
        }

        // Clear constraints
        try
        {
            var physicsRig = RigData.Refs.RigManager.physicsRig;
            var constraintTrackers = physicsRig.GetComponentsInChildren<ConstraintTracker>();

            foreach (var tracker in constraintTrackers)
            {
                tracker.DeleteConstraint();
            }
        }
        catch (Exception e)
        {
            FusionLogger.LogException("deleting constraints on local player", e);
        }
    }

    /// <summary>
    /// Causes the Local Player to release everything they are currently grabbing.
    /// </summary>
    public static void ReleaseGrips()
    {
        if (!RigData.HasPlayer)
        {
            return;
        }

        var physicsRig = RigData.Refs.RigManager.physicsRig;

        physicsRig.leftHand.TryDetach();
        physicsRig.rightHand.TryDetach();
    }
}