using HarmonyLib;

using Il2CppSLZ.Marrow;

using LabFusion.Entities;
using LabFusion.Marrow.Extenders;
using LabFusion.Marrow.Messages;
using LabFusion.Network;
using LabFusion.Scene;

namespace LabFusion.Marrow.Patching;

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

        var networkRig = networkEntity.GetExtender<NetworkRig>();

        if (networkRig == null)
        {
            return;
        }

        var physicsRig = networkRig.RigRefs.RigManager.physicsRig;

        bool left = __instance == physicsRig.legLf;

        var data = new PhysicsRigStateData()
        {
            RigReference = new(networkEntity),
            Type = PhysicsRigStateType.LegShutdown,
            Enabled = true,
            Left = left,
        };

        MessageRelay.RelayModule<PhysicsRigStateMessage, PhysicsRigStateData>(data, CommonMessageRoutes.ReliableToOtherClients);
    }
}
