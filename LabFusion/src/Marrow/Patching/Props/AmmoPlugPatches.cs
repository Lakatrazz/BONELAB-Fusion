using HarmonyLib;

using Il2CppSLZ.Marrow;

using LabFusion.Marrow.Extenders;
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

        var socket = __instance._lastSocket;

        if (socket != null && socket.IsClearOnInsert)
        {
            CompleteDespawn(__instance);

            PooleeDespawnPatch.IgnorePatch = true;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(AmmoPlug.OnPlugInsertComplete))]
    public static void OnPlugInsertCompletePostfix(AmmoPlug __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        PooleeDespawnPatch.IgnorePatch = false;
    }

    private static void CompleteDespawn(AmmoPlug ammoPlug)
    {
        // Even though the game despawns the ammo locally, a despawn message is still sent
        // This is to ensure it gets despawned for all clients, even if they don't have a gun on their end or the ammo is only a ghost
        if (!AmmoPlugExtender.Cache.TryGet(ammoPlug, out var networkEntity))
        {
            return;
        }

        if (!networkEntity.IsOwner)
        {
            return;
        }

        NetworkAssetSpawner.Despawn(new NetworkAssetSpawner.DespawnRequestInfo()
        {
            EntityID = networkEntity.ID,
            DespawnEffect = false,
        });
    }
}