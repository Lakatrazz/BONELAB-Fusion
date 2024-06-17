using HarmonyLib;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.PuppetMasta;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(PuppetMaster))]
public static class PuppetMasterPatches
{
    public static bool IgnorePatches = false;

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

        using (var writer = FusionWriter.Create(PropReferenceData.Size))
        {
            var data = PropReferenceData.Create(PlayerIdManager.LocalSmallId, entity.Id);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.PuppetMasterKill, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }

        PuppetMasterExtender.LastKilled = entity;
    }
}