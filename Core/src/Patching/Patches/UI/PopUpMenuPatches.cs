using Il2CppSystem;

using HarmonyLib;

using SLZ.UI;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.RPC;
using SLZ.Marrow.Data;

namespace LabFusion.Patching
{

    [HarmonyPatch(typeof(PopUpMenuView), nameof(PopUpMenuView.AddDevMenu))]
    public static class AddDevMenuPatch
    {
        public static void Prefix(PopUpMenuView __instance, ref Action spawnDelegate)
        {
            spawnDelegate += (Action)(() => { OnSpawnDelegate(__instance); });
        }

        public static void OnSpawnDelegate(PopUpMenuView __instance)
        {
            if (NetworkInfo.HasServer && !NetworkInfo.IsServer && RigData.HasPlayer && RigData.RigReferences.RigManager.uiRig.popUpMenu == __instance)
            {
                var transform = __instance.radialPageView.transform;

                var spawnGun = new Spawnable() { crateRef = new(__instance.crate_SpawnGun.Barcode) };
                var nimbusGun = new Spawnable() { crateRef = new(__instance.crate_Nimbus.Barcode) };

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
    }
}
