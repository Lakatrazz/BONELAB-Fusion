using System;
using System.Collections.Generic;
using System.Linq;

using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;

using UnityEngine;

namespace LabFusion.Marrow
{
    public static class SafeAssetSpawner
    {
        public static void Spawn(Spawnable spawnable, Vector3 position, Quaternion rotation, Action<Poolee> spawnCallback = null)
        {
            Spawn(spawnable.crateRef.Barcode, position, rotation, spawnCallback);
        }

        public static void Spawn(Barcode barcode, Vector3 position, Quaternion rotation, Action<Poolee> spawnCallback = null)
        {
            var spawnerInstance = AssetSpawner._instance;

            var barcodeToPool = spawnerInstance._barcodeToPool;

            // Make sure the pool exists in the dictionary
            if (!barcodeToPool.ContainsKey(barcode))
            {
                return;
            }

            // Get the pool from the barcode
            var pool = barcodeToPool[barcode];

            var spawnTask = pool.Spawn(position, rotation, new(Vector3.one));
            var awaiter = spawnTask.GetAwaiter();

            var continuation = () =>
            {
                var result = awaiter.GetResult();

                spawnCallback?.Invoke(result);
            };
            awaiter.OnCompleted(continuation);
        }
    }
}
