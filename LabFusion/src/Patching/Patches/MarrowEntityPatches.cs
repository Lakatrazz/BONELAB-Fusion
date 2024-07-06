using HarmonyLib;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Zones;

using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Marrow;
using LabFusion.Network;
using LabFusion.Utilities;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(MarrowEntity))]
public static class MarrowEntityPatches
{
    public static readonly ComponentHashTable<MarrowEntity> HashTable = new();

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MarrowEntity.Awake))]
    public static void Awake(MarrowEntity __instance)
    {
        var hash = GameObjectHasher.GetHierarchyHash(__instance.gameObject);

        var index = HashTable.AddComponent(hash, __instance);

#if DEBUG
        if (index > 0)
        {
            FusionLogger.Log($"Entity {__instance.name} had a conflicting hash {hash} and has been added at index {index}.");
        }
#endif
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MarrowEntity.OnDestroy))]
    public static void OnDestroy(MarrowEntity __instance)
    {
        HashTable.RemoveComponent(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(MarrowEntity.OnCullResolve))]
    public static void OnCullResolve(MarrowEntity __instance, InactiveStatus status, bool isInactive)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var entity = IMarrowEntityExtender.Cache.Get(__instance);

        if (entity == null)
        {
            return;
        }

        var extender = entity.GetExtender<IMarrowEntityExtender>();

        extender.OnEntityCull(isInactive);
    }
}
