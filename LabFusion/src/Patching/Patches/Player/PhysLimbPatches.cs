using HarmonyLib;

using Il2CppSLZ.Marrow;

using LabFusion.Entities;
using LabFusion.Marrow.Extenders;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Scene;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(PhysLimb))]
public static class PhysLimbPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PhysLimb.ShutdownLimb))]
    public static void ShutdownLimb(PhysLimb __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        var networkEntity = PhysLimbExtender.Cache.Get(__instance);

        if (networkEntity == null || !networkEntity.IsOwner)
        {
            return;
        }

        var networkPlayer = networkEntity.GetExtender<NetworkPlayer>();

        if (networkPlayer == null)
        {
            return;
        }

        var physicsRig = networkPlayer.RigRefs.RigManager.physicsRig;

        bool left = __instance == physicsRig.legLf;

        var data = PhysicsRigStateData.Create(PlayerIDManager.LocalSmallID, PhysicsRigStateType.LEG_SHUTDOWN, true, left);

        MessageRelay.RelayNative(data, NativeMessageTag.PhysicsRigState, CommonMessageRoutes.ReliableToOtherClients);
    }
}
