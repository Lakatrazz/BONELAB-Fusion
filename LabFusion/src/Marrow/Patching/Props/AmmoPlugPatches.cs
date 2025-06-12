using HarmonyLib;

using Il2CppSLZ.Marrow;

using LabFusion.Entities;
using LabFusion.RPC;
using LabFusion.Scene;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(AmmoPlug))]
public static class AmmoPlugPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(AmmoPlug.OnPlugInsertComplete))]
    public static void OnPlugInsertCompletePrefix(AmmoPlug __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        if (__instance.magazine == null)
        {
            return;
        }
        
        if (!MagazineExtender.Cache.TryGet(__instance.magazine, out var networkEntity))
        {
            return;
        }

        var socket = __instance._lastSocket;

        if (socket != null && socket.IsClearOnInsert)
        {
            NetworkAssetSpawner.Despawn(new NetworkAssetSpawner.DespawnRequestInfo()
            {
                EntityID = networkEntity.ID,
                DespawnEffect = false,
            });
        }
    }
}