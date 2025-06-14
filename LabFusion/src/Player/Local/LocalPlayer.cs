#nullable enable

using Il2CppSLZ.Marrow;

using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Player;

public delegate void PlayerGrabDelegate(Hand hand, Grip grip);

public static class LocalPlayer
{
    public static PlayerGrabDelegate? OnGrab { get; set; }
    public static PlayerGrabDelegate? OnRelease { get; set; }

    public static Action<RigManager>? OnLocalRigCreated { get; set; }

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

            Metadata.Username.SetValue(value);

            OnUsernameChanged?.InvokeSafe(value, "executing OnUsernameChanged");
        }
    }

    public static event Action<string>? OnUsernameChanged;

    public static event Action? OnApplyInitialMetadata;

    public static PlayerMetadata Metadata { get; } = new();

    internal static void OnInitializeMelon()
    {
        Metadata.CreateMetadata();

        Metadata.Metadata.OnTrySetMetadata += OnTrySetMetadata;
        Metadata.Metadata.OnTryRemoveMetadata += OnTryRemoveMetadata;

        LocalAvatar.OnInitializeMelon();
        LocalHealth.OnInitializeMelon();
        LocalVision.OnInitializeMelon();
        LocalControls.OnInitializeMelon();
    }

    internal static void OnFixedUpdate()
    {
        LocalControls.OnFixedUpdate();
    }

    private static bool OnTrySetMetadata(string key, string value)
    {
        Metadata.Metadata.ForceSetLocalMetadata(key, value);

        var localId = PlayerIDManager.LocalID;

        localId?.Metadata.Metadata.TrySetMetadata(key, value);

        return true;
    }

    private static bool OnTryRemoveMetadata(string key)
    {
        Metadata.Metadata.ForceRemoveLocalMetadata(key);

        var localId = PlayerIDManager.LocalID;

        localId?.Metadata.Metadata.TryRemoveMetadata(key);

        return true;
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

        if (NetworkPlayerManager.TryGetPlayer(PlayerIDManager.LocalID, out var player))
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

    /// <summary>
    /// Teleports the Local Player to their checkpoint.
    /// </summary>
    public static void TeleportToCheckpoint()
    {
        if (!RigData.HasPlayer)
        {
            return;
        }

        var rigManager = RigData.Refs.RigManager;

        TeleportToPosition(rigManager.checkpointPosition, rigManager.checkpointFwd);
    }

    /// <summary>
    /// Teleports the Local Player to a set position.
    /// </summary>
    /// <param name="position">The point to teleport to in world space.</param>
    public static void TeleportToPosition(Vector3 position)
    {
        if (!RigData.HasPlayer)
        {
            return;
        }

        var rigManager = RigData.Refs.RigManager;

        rigManager.TeleportToPosition(position, true);
    }

    /// <summary>
    /// Teleports the Local Player to a set position and forward direction.
    /// </summary>
    /// <param name="position">The point to teleport to in world space.</param>
    /// <param name="forward">The forward direction that the player will face in world space.</param>
    public static void TeleportToPosition(Vector3 position, Vector3 forward)
    {
        if (!RigData.HasPlayer)
        {
            return;
        }

        var rigManager = RigData.Refs.RigManager;

        rigManager.TeleportToPosition(position, forward, true);
    }
}