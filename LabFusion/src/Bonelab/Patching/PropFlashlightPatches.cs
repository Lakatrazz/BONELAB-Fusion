using HarmonyLib;

using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Bonelab.Extenders;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(PropFlashlight))]
public static class PropFlashlightPatches
{
    public static bool IgnorePatches = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PropFlashlight.SwitchLight))]
    public static bool SwitchLightPrefix(PropFlashlight __instance)
    {
        if (IgnorePatches)
        {
            return true;
        }

        if (NetworkInfo.HasServer && PropFlashlightExtender.Cache.TryGet(__instance, out var entity) && !entity.IsOwner)
        {
            return false;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PropFlashlight.SwitchLight))]
    public static void SwitchLightPostfix(PropFlashlight __instance)
    {
        if (IgnorePatches)
        {
            return;
        }

        if (NetworkInfo.HasServer && PropFlashlightExtender.Cache.TryGet(__instance, out var entity))
        {
            using var writer = FusionWriter.Create(FlashlightToggleData.Size);
            var data = FlashlightToggleData.Create(PlayerIdManager.LocalSmallId, entity.Id, __instance.lightOn);
            writer.Write(data);

            using var message = FusionMessage.ModuleCreate<FlashlightToggleMessage>(writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }
    }
}