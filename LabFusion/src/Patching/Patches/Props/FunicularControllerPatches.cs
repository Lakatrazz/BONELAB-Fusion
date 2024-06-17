using HarmonyLib;

using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Entities;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(FunicularController))]
public static class FunicularControllerPatches
{
    public static bool IgnorePatches = false;

    private static bool OnFunicularEvent(FunicularController __instance, FunicularControllerEventType type)
    {
        if (IgnorePatches)
        {
            return true;
        }

        ushort syncId = 0;

        if (FunicularControllerExtender.Cache.TryGet(__instance, out var entity))
        {
            syncId = entity.Id;
        }

        return QuickSender.SendServerMessage(() =>
        {
            PowerableSender.SendFunicularControllerEvent(syncId, type);
        });
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(FunicularController.CartGo))]
    public static bool CartGo(FunicularController __instance)
    {
        return OnFunicularEvent(__instance, FunicularControllerEventType.CARTGO);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(FunicularController.CartForwards))]
    public static bool CartForwards(FunicularController __instance)
    {
        return OnFunicularEvent(__instance, FunicularControllerEventType.CARTFORWARDS);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(FunicularController.CartBackwards))]
    public static bool CartBackwards(FunicularController __instance)
    {
        return OnFunicularEvent(__instance, FunicularControllerEventType.CARTBACKWARDS);
    }
}