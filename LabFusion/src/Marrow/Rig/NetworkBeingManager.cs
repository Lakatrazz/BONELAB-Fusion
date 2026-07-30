using Il2CppSLZ.Marrow;

using LabFusion.Entities;
using LabFusion.Scene;
using LabFusion.Utilities;

namespace LabFusion.Marrow.Rig;

/// <summary>
/// Manager class for interacting with networked beings that use a RigManager, such as players, NPCs, and ragdolls.
/// </summary>
public static class NetworkBeingManager
{
    /// <summary>
    /// Checks if the local player has ownership over a RigManager.
    /// <para>The local player can have ownership if the RigManager is the player themselves or if they last interacted with it.</para>
    /// <para>It will also return true if the rig is not networked and is client side only.</para>
    /// </summary>
    /// <param name="rigManager"></param>
    /// <returns></returns>
    public static bool HasOwnership(this RigManager rigManager)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        if (rigManager.IsLocalPlayer())
        {
            return true;
        }

        if (!TryGetNetworkRig(rigManager, out var networkRig))
        {
            return true;
        }

        return networkRig.NetworkEntity.IsOwner;
    }

    /// <summary>
    /// Attempts to find a NetworkRig from a RigManager. If the RigManager is null or client side only, the function will return false.
    /// </summary>
    /// <param name="rigManager"></param>
    /// <param name="networkRig"></param>
    /// <returns></returns>
    public static bool TryGetNetworkRig(RigManager rigManager, out NetworkRig networkRig)
    {
        if (rigManager == null)
        {
            networkRig = null;
            return false;
        }

        return NetworkRig.Cache.TryGet(rigManager, out networkRig);
    }
}
