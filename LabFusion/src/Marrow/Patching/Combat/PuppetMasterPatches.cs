using HarmonyLib;

using LabFusion.Network;
using LabFusion.Entities;
using LabFusion.Marrow.Extenders;
using LabFusion.Marrow.Messages;
using LabFusion.Scene;

using Il2CppSLZ.Marrow.PuppetMasta;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(PuppetMaster))]
public static class PuppetMasterPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PuppetMaster.Kill))]
    [HarmonyPatch(new Type[] { })]
    public static bool KillPrefix(PuppetMaster __instance) => ValidateKill(__instance);

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PuppetMaster.Kill))]
    [HarmonyPatch(new Type[] { typeof(PuppetMaster.StateSettings) })]
    public static bool KillPrefix(PuppetMaster __instance, PuppetMaster.StateSettings stateSettings) => ValidateKill(__instance);

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PuppetMaster.PostKill))]
    public static void PostKill(PuppetMaster __instance)
    {
        if (IgnorePatches)
        {
            return;
        }

        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        if (!PuppetMasterExtender.Cache.TryGet(__instance, out var entity) || !entity.IsOwner)
        {
            return;
        }

        var data = new NetworkEntityReference(entity);

        MessageRelay.RelayModule<PuppetMasterKillMessage, NetworkEntityReference>(data, CommonMessageRoutes.ReliableToOtherClients);

        PuppetMasterExtender.LastKilled = entity;
    }

    private static bool ValidateKill(PuppetMaster puppetMaster)
    {
        if (IgnorePatches)
        {
            return true;
        }

        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        if (!PuppetMasterExtender.Cache.TryGet(puppetMaster, out var entity))
        {
            return true;
        }

        if (!entity.IsOwner)
        {
            return false;
        }

        return true;
    }
}