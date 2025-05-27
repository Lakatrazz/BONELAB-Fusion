using HarmonyLib;

using LabFusion.RPC;
using LabFusion.Scene;

using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Bonelab;

using Action = Il2CppSystem.Action;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(PopUpMenuView))]
public static class PopUpMenuViewPatches
{
    public static bool DisableMethods { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PopUpMenuView.AddSpawnMenu))]
    public static bool AddSpawnMenuPrefix()
    {
        if (DisableMethods)
        {
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PopUpMenuView.RemoveSpawnMenu))]
    public static bool RemoveSpawnMenuPrefix()
    {
        if (DisableMethods)
        {
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PopUpMenuView.AddDevMenu))]
    public static void AddDevMenuPrefix(PopUpMenuView __instance, ref Action spawnDelegate)
    {
        // Completely override the spawn delegate with our networked version
        var originalDelegate = Action.Combine(spawnDelegate).TryCast<Action>();

        spawnDelegate = (Action)(() => { OnSpawnDelegate(__instance, originalDelegate); });
    }

    public static void OnSpawnDelegate(PopUpMenuView menu, Action originalDelegate)
    {
        // If there is no server, we can just spawn the original items as normal
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            originalDelegate?.Invoke();
            return;
        }

        var transform = menu.radialPageView.transform;

        var spawnGun = new Spawnable() { crateRef = new(menu.crate_SpawnGun.Barcode) };
        var nimbusGun = new Spawnable() { crateRef = new(menu.crate_Nimbus.Barcode) };

        var spawnGunInfo = new NetworkAssetSpawner.SpawnRequestInfo()
        {
            Spawnable = spawnGun,
            Position = transform.position,
            Rotation = transform.rotation
        };

        var nimbusGunInfo = new NetworkAssetSpawner.SpawnRequestInfo()
        {
            Spawnable = nimbusGun,
            Position = transform.position,
            Rotation = transform.rotation
        };

        NetworkAssetSpawner.Spawn(spawnGunInfo);
        NetworkAssetSpawner.Spawn(nimbusGunInfo);
    }
}