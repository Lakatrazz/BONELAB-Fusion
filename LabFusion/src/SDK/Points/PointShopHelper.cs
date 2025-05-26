using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;

using LabFusion.Marrow;

using UnityEngine;

namespace LabFusion.SDK.Points;

public static class PointShopHelper
{
    public static void SpawnBitMart(Vector3 position, Quaternion rotation)
    {
        var spawnable = new Spawnable()
        {
            crateRef = FusionSpawnableReferences.BitMartReference,
            policyData = null,
        };

        AssetSpawner.Register(spawnable);

        SafeAssetSpawner.Spawn(spawnable, position, rotation);
    }
}