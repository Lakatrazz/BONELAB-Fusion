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

            var success = spawnerInstance._barcodeToPool.TryGetValue(barcode, out var pool);

            if (!success)
            {
                return;
            }

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
