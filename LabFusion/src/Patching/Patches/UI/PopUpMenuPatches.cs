using HarmonyLib;

using LabFusion.Network;
using LabFusion.RPC;

using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Bonelab;

using Action = Il2CppSystem.Action;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(PopUpMenuView))]
public static class AddSpawnMenuPatches
{
    public static bool DisableMethods = false;

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
}

[HarmonyPatch(typeof(PopUpMenuView), nameof(PopUpMenuView.AddDevMenu))]
public static class AddDevMenuPatch
{
    public static void Prefix(PopUpMenuView __instance, ref Action spawnDelegate)
    {
        // Completely override the spawn delegate with our networked version
        var originalDelegate = Action.Combine(spawnDelegate).TryCast<Action>();

        spawnDelegate = (Action)(() => { OnSpawnDelegate(__instance, originalDelegate); });
    }

    public static void OnSpawnDelegate(PopUpMenuView menu, Action originalDelegate)
    {
        // If there is no server, we can just spawn the original items as normal
        if (!NetworkInfo.HasServer)
        {
            originalDelegate?.Invoke();
            return;
        }

        var transform = menu.radialPageView.transform;

        var spawnGun = new Spawnable() { crateRef = new(menu.crate_SpawnGun.Barcode) };
        var nimbusGun = new Spawnable() { crateRef = new(menu.crate_Nimbus.Barcode) };

        var spawnGunInfo = new NetworkAssetSpawner.SpawnRequestInfo()
        {
            spawnable = spawnGun,
            position = transform.position,
            rotation = transform.rotation
        };

        var nimbusGunInfo = new NetworkAssetSpawner.SpawnRequestInfo()
        {
            spawnable = nimbusGun,
            position = transform.position,
            rotation = transform.rotation
        };

        NetworkAssetSpawner.Spawn(spawnGunInfo);
        NetworkAssetSpawner.Spawn(nimbusGunInfo);
    }
}