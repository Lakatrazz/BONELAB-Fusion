using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;

using UnityEngine;

namespace LabFusion.Marrow
{
    public static class SafeAssetSpawner
    {
        public static void Spawn(Spawnable spawnable, Vector3 position, Quaternion rotation, Action<Poolee> spawnCallback = null)
        {
            // spawnCallback and despawnCallback no longer seem to work in patch 4 through MelonLoader
            // Instead, we use the async variant and simply continue when the method finishes
            // This originally spawned directly from the pool, however that seemed to have strange effects on spawned objects
            var spawnTask = AssetSpawner.SpawnAsync(spawnable, position, rotation, new(Vector3.one), false, new(0), null, null);
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
