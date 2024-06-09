using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;

using UnityEngine;

namespace LabFusion.SDK.Points
{
    public static class PointShopHelper
    {
        public const string BitMartBarcode = "Lakatrazz.FusionContent.Spawnable.BitMart";

        public static void CompleteBitMart(GameObject gameObject)
        {
            // Currently just needs to add the PointShop script
            gameObject.AddComponent<PointShop>();
        }

        public static void SpawnBitMart(Vector3 position, Quaternion rotation)
        {
            var spawnable = new Spawnable()
            {
                crateRef = new SpawnableCrateReference(BitMartBarcode),
                policyData = null,
            };

            AssetSpawner.Register(spawnable);

            AssetSpawner.Spawn(spawnable, position, rotation, new Il2CppSystem.Nullable<Vector3>(Vector3.one), true, new Il2CppSystem.Nullable<int>(0), null, null);
        }
    }
}