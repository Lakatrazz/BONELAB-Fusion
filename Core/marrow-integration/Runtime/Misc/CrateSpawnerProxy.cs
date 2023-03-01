using System;

using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.Network;

using SLZ.Marrow.Pool;
using SLZ.Marrow.Warehouse;
#endif

namespace LabFusion.MarrowIntegration {
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [AddComponentMenu("BONELAB Fusion/Misc/Crate Spawner Proxy")]
    [DisallowMultipleComponent]
#endif
    public sealed class CrateSpawnerProxy : FusionMarrowBehaviour {
#if MELONLOADER
        public CrateSpawnerProxy(IntPtr intPtr) : base(intPtr) { }

        public void DespawnAll(string barcode) { 
            if (!NetworkInfo.HasServer || NetworkInfo.IsServer) {
                var barcodeToPool = AssetSpawner._instance._barcodeToPool;
                var newBarcode = new Barcode(barcode);

                if (barcodeToPool.TryGetValue(newBarcode, out var pool)) {
                    var spawnedObjects = pool.spawned.ToArray();

                    foreach (var spawned in spawnedObjects) {
                        spawned.Despawn();
                    }
                } 
            }
        }

#else
        public override string Comment => "This proxy lets you manually control events of spawned crates in the scene.\n" +
            "For example, through a UnityEvent or UltEvent you could despawn every object of a specific barcode (see DespawnAll(string barcode)).\n" +
            "This may be useful for gamemodes that end and you need all of the items to vanish.";

        public void DespawnAll(string barcode) { }
#endif
    }
}
