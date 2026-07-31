using UnityEngine;

using LabFusion.Utilities;
using LabFusion.Player;
using LabFusion.Entities;
using LabFusion.Marrow;
using LabFusion.Extensions;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Data;

public static class RigData
{
    public static RigRefs Refs { get; private set; } = new RigRefs();
    public static bool HasPlayer => Refs.IsValid;

    public static Vector3 RigSpawn { get; private set; }
    public static Quaternion RigSpawnRot { get; private set; }

    public static void OnCacheRigInfo()
    {
        var playerRefs = PlayerRefs.Instance;

        if (playerRefs == null)
        {
            FusionLogger.Error("PlayerRefs does not exist, cannot get the player's RigManager!");
            return;
        }

        var manager = playerRefs.PlayerRigManager;

        if (manager == null)
        {
            FusionLogger.Error("Failed to get the player's RigManager!");
            return;
        }

        // Store spawn values
        RigSpawn = manager.transform.position;
        RigSpawnRot = manager.transform.rotation;

        // Store the references
        Refs = new RigRefs(manager);

        // Notify hooks
        LocalPlayer.OnLocalRigCreated?.InvokeSafe(manager, "executing OnLocalRigCreated hook");

        // Update avatar
        if (manager._avatar != null)
        {
            LocalAvatar.InvokeAvatarChanged(manager._avatar, manager.AvatarCrate.Barcode.ID);
        }
    }
}