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
    public sealed class CrateSpawnerProxy : MonoBehaviour {
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
        public void DespawnAll(string barcode) { }
#endif
    }
}
