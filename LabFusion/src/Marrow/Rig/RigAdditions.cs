using Il2CppSLZ.Marrow;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Marrow.Rig;

/// <summary>
/// Provides events to allow for the modification of RigManagers, whether general, local player only, or net players only.
/// </summary>
public static class RigAdditions
{
    /// <summary>
    /// Invoked when modifications for any rig are to be made.
    /// </summary>
    public static event Action<RigManager> ApplyingRigAdditions;

    /// <summary>
    /// Invoked when modifications for any rig are to be removed.
    /// </summary>
    public static event Action<RigManager> RemovingRigAdditions;

    /// <summary>
    /// Invoked when modifications for specifically the local player are to be made.
    /// </summary>
    public static event Action<RigManager> ApplyingLocalRigAdditions;

    /// <summary>
    /// Invoked when modifications for specifically the local player are to be removed.
    /// </summary>
    public static event Action<RigManager> RemovingLocalRigAdditions;

    /// <summary>
    /// Invoked when modifications for specifically net players are to be made.
    /// </summary>
    public static event Action<RigManager> ApplyingNetRigAdditions;

    /// <summary>
    /// Invoked when modifications for specifically net players are to be removed.
    /// </summary>
    public static event Action<RigManager> RemovingNetRigAdditions;

    internal static void Initialize()
    {
        MultiplayerHooking.OnJoinedServer += ApplyAdditions;
        MultiplayerHooking.OnStartedServer += ApplyAdditions;
        MultiplayerHooking.OnDisconnected += RemoveAdditions;
        LocalPlayer.OnLocalRigCreated += OnLocalRigCreated;

        void ApplyAdditions()
        {
            if (!RigData.HasPlayer)
            {
                return;
            }

            ApplyLocalRigAdditions(RigData.Refs.RigManager);
        }

        void RemoveAdditions()
        {
            if (!RigData.HasPlayer)
            {
                return;
            }

            RemoveLocalRigAdditions(RigData.Refs.RigManager);
        }
    }

    private static void OnLocalRigCreated(RigManager rigManager)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        ApplyLocalRigAdditions(rigManager);
    }

    public static void ApplyRigAdditions(RigManager rigManager)
    {
        ApplyingRigAdditions?.Invoke(rigManager);
    }

    public static void RemoveRigAdditions(RigManager rigManager)
    {
        RemovingRigAdditions?.Invoke(rigManager);
    }

    public static void ApplyLocalRigAdditions(RigManager rigManager)
    {
        ApplyRigAdditions(rigManager);

        ApplyingLocalRigAdditions?.Invoke(rigManager);
    }

    public static void RemoveLocalRigAdditions(RigManager rigManager)
    {
        RemoveRigAdditions(rigManager);

        RemovingLocalRigAdditions?.Invoke(rigManager);
    }

    public static void ApplyNetRigAdditions(RigManager rigManager)
    {
        ApplyRigAdditions(rigManager);

        ApplyingNetRigAdditions?.Invoke(rigManager);
    }

    public static void RemoveNetRigAdditions(RigManager rigManager)
    {
        RemoveRigAdditions(rigManager);

        RemovingNetRigAdditions?.Invoke(rigManager);
    }
}
