using HarmonyLib;

using LabFusion.Network;

using MarrowFusion.Bonelab.Extenders;
using MarrowFusion.Bonelab.Messages;

using Il2CppSLZ.Bonelab;

namespace MarrowFusion.Bonelab.Patching;

[HarmonyPatch(typeof(PropFlashlight))]
public static class PropFlashlightPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PropFlashlight.SwitchLight))]
    public static bool SwitchLightPrefix(PropFlashlight __instance)
    {
        if (IgnorePatches)
        {
            IgnorePatches = false;
            return true;
        }

        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        if (!PropFlashlightExtender.Cache.TryGet(__instance, out var entity))
        {
            return true;
        }

        if (entity.IsOwner)
        {
            var toggledLight = !__instance.lightOn;

            var data = FlashlightToggleData.Create(new(entity), toggledLight);

            MessageRelay.RelayModule<FlashlightToggleMessage, FlashlightToggleData>(data, CommonMessageRoutes.ReliableToClients);
        }

        return false;
    }
}