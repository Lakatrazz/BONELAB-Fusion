using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Zones;

using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.Network;
using LabFusion.Utilities;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(MarrowEntity))]
public static class MarrowEntityPatches
{
    public static readonly FusionDictionary<int, List<MarrowEntity>> HashToEntities = new();
    public static readonly FusionDictionary<MarrowEntity, int> EntityToHash = new(new UnityComparer());

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MarrowEntity.Awake))]
    public static void Awake(MarrowEntity __instance)
    {
        var hash = GameObjectHasher.GetHierarchyHash(__instance.gameObject);

        if (!HashToEntities.TryGetValue(hash, out var entities))
        {
            entities = new();
            HashToEntities.Add(hash, entities);
        }

        entities.Add(__instance);
        EntityToHash.Add(__instance, hash);

#if DEBUG
        if (entities.Count > 1)
        {
            FusionLogger.Log($"Entity {__instance.name} had a conflicting hash {hash} and has been added at index {HashToEntities[hash].Count - 1}.");
        }
#endif
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MarrowEntity.OnDestroy))]
    public static void OnDestroy(MarrowEntity __instance)
    {
        if (!EntityToHash.TryGetValue(__instance, out var hash))
        {
            return;
        }

        EntityToHash.Remove(__instance);

        if (HashToEntities.TryGetValue(hash, out var entities))
        {
            // Regular remove will not work for IL2CPP objects
            // So we use RemoveAll
            entities.RemoveAll((e) => e == __instance);

            if (entities.Count <= 0)
            {
                HashToEntities.Remove(hash);
            }
        }
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
