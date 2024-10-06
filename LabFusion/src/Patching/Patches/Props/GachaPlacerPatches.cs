using HarmonyLib;

using LabFusion.Network;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Props;

using UnityEngine;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(GachaPlacer))]
public static class GachaPlacerPatches
{
    public static bool IgnorePatches = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GachaPlacer.OnPersistentLoad))]
    public static bool OnPersistentLoad(GachaPlacer __instance)
    {
        if (IgnorePatches)
        {
            return true;
        }
        
        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        // If we're in a server, make sure the capsule always spawns unless its manual mode
        var crateSpawner = __instance._crateSpawner;

        if (!crateSpawner.manualMode)
        {
            crateSpawner.SpawnSpawnable();
        }

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GachaPlacer.OnSpawn))]
    public static void OnSpawn(GachaPlacer __instance, GameObject go)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        IgnorePatches = true;

        bool notUnlocked = __instance.ShouldSpawn();

        if (!notUnlocked)
        {
            MeshRenderer mesh = go.GetComponent<GachaCapsule>().previewMesh.GetComponent<MeshRenderer>();
            mesh.material.SetColor("_EmissionColor", Color.gray);
            mesh.material.SetColor("_HologramEdgeColor", Color.gray);
        }

        IgnorePatches = false;
    }
}