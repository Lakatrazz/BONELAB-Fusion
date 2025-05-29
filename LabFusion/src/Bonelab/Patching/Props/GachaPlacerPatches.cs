using HarmonyLib;

using LabFusion.Utilities;
using LabFusion.Scene;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Props;

using UnityEngine;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(GachaPlacer))]
public static class GachaPlacerPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GachaPlacer.OnPersistentLoad))]
    public static bool OnPersistentLoad(GachaPlacer __instance)
    {
        if (IgnorePatches)
        {
            return true;
        }
        
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        // If we're in a server, make sure the gachapon always spawns
        var crateSpawner = __instance._crateSpawner;

        crateSpawner.SpawnSpawnable();

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GachaPlacer.OnSpawn))]
    public static void OnSpawn(GachaPlacer __instance, GameObject go)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        IgnorePatches = true;

        try
        {
            bool notUnlocked = __instance.ShouldSpawn();

            if (!notUnlocked)
            {
                MeshRenderer mesh = go.GetComponent<GachaCapsule>().previewMesh.GetComponent<MeshRenderer>();
                mesh.material.SetColor("_EmissionColor", Color.gray);
                mesh.material.SetColor("_HologramEdgeColor", Color.gray);
            }
        }
        catch (Exception e)
        {
            FusionLogger.LogException("overriding GachaPlacer color", e);
        }

        IgnorePatches = false;
    }
}