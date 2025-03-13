using HarmonyLib;

using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.PuppetMasta;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(PuppetMaster))]
public static class PuppetMasterPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPatch(nameof(PuppetMaster.PostKill))]
    [HarmonyPrefix]
    public static void PostKill(PuppetMaster __instance)
    {
        if (IgnorePatches)
        {
            return;
        }

        if (!NetworkInfo.HasServer)
        {
            return;
        }

        if (!PuppetMasterExtender.Cache.TryGet(__instance, out var entity) || !entity.IsOwner)
        {
            return;
        }

        var data = new NetworkEntityReference(entity);

        MessageRelay.RelayNative(data, NativeMessageTag.PuppetMasterKill, NetworkChannel.Reliable, RelayType.ToOtherClients);

        PuppetMasterExtender.LastKilled = entity;
    }
}