using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Marrow;

using UnityEngine;

namespace LabFusion.SDK.Points
{
    public static class PointShopHelper
    {
        public static void CompleteBitMart(GameObject gameObject)
        {
            // Currently just needs to add the PointShop script
            gameObject.AddComponent<PointShop>();
        }

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
}