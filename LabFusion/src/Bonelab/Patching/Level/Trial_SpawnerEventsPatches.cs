using HarmonyLib;

using LabFusion.Network;
using LabFusion.Data;
using LabFusion.Marrow;
using LabFusion.Utilities;
using LabFusion.Bonelab.Messages;
using LabFusion.Scene;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(Trial_SpawnerEvents))]
public static class Trial_SpawnerEventsPatches
{
    public static bool IgnorePatches { get; set; } = false;

    public static readonly ComponentHashTable<Trial_SpawnerEvents> HashTable = new();

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Trial_SpawnerEvents.OnEnable))]
    public static void OnEnablePrefix(Trial_SpawnerEvents __instance)
    {
        var hash = GameObjectHasher.GetHierarchyHash(__instance.gameObject);

        var index = HashTable.AddComponent(hash, __instance);

#if DEBUG
        if (index > 0)
        {
            FusionLogger.Log($"Trial_SpawnerEvents {__instance.name} had a conflicting hash {hash} and has been added at index {index}.");
        }
#endif
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Trial_SpawnerEvents.OnDisable))]
    public static void OnDisablePrefix(Trial_SpawnerEvents __instance)
    {
        HashTable.RemoveComponent(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Trial_SpawnerEvents.OnSpawnerDeath))]
    public static bool OnSpawnerDeath(Trial_SpawnerEvents __instance)
    {
        if (IgnorePatches)
        {
            IgnorePatches = false;
            return true;
        }

        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        if (NetworkSceneManager.IsLevelHost)
        {
            var hashData = HashTable.GetDataFromComponent(__instance);

            MessageRelay.RelayModule<TrialSpawnerEventsMessage, TrialSpawnerEventsData>(new() { HashData = hashData }, CommonMessageRoutes.ReliableToClients);
        }

        return false;
    }
}
