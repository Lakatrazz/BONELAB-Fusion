using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;

using UnityEngine;

namespace LabFusion.Marrow.Pool;

public static class LocalAssetSpawner
{
    public static Spawnable CreateSpawnable(SpawnableCrateReference crateReference) => new() { crateRef = crateReference, policyData = null };

    public static Spawnable CreateSpawnable(string barcode) => new() { crateRef = new(barcode), policyData = null };

    public static void Register(Spawnable spawnable)
    {
        AssetSpawner.Register(spawnable);
    }

    public static void Spawn(Spawnable spawnable, Vector3 position, Quaternion rotation, Action<Poolee> spawnCallback = null)
    {
        // spawnCallback and despawnCallback no longer seem to work in patch 4 through MelonLoader
        // Instead, we use the async variant and simply continue when the method finishes
        // This originally spawned directly from the pool, however that seemed to have strange effects on spawned objects
        var scale = new Il2CppSystem.Nullable<Vector3>(Vector3.zero)
        {
            hasValue = false,
        };

        var groupId = new Il2CppSystem.Nullable<int>(0)
        {
            hasValue = false,
        };

        var spawnTask = AssetSpawner.SpawnAsync(spawnable, position, rotation, scale, null, false, groupId, null, null);
        var awaiter = spawnTask.GetAwaiter();

        var continuation = () =>
        {
            var result = awaiter.GetResult();

            // The GameObject did not spawn, so we don't have to invoke the callback
            // Maybe pass in a Failed result in the future instead?
            if (result == null)
            {
                return;
            }

            spawnCallback?.Invoke(result);
        };
        awaiter.OnCompleted(continuation);
    }
}