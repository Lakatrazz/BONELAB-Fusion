#nullable enable

using Il2CppSLZ.Marrow;

using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Utilities;

namespace LabFusion.Player;

public delegate void PlayerGrabDelegate(Hand hand, Grip grip);

public static class LocalPlayer
{
    public static PlayerGrabDelegate? OnGrab { get; set; }
    public static PlayerGrabDelegate? OnRelease { get; set; }

    public static Action<RigManager>? OnLocalRigCreated { get; set; }

    public static bool RagdollOnDeath => NetworkInfo.HasServer;

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

    public static void ClearConstraints()
    {
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
}