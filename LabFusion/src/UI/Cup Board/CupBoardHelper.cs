using UnityEngine;

using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;

using LabFusion.Marrow;

namespace LabFusion.UI
{
    public static class CupBoardHelper
    {
        public static void CompleteAchievementBoard(GameObject gameObject)
        {
            // Currently just needs to add the CupBoard script
            gameObject.AddComponent<CupBoard>();
        }

        public static void SpawnAchievementBoard(Vector3 position, Quaternion rotation)
        {
            var spawnable = new Spawnable()
            {
                crateRef = FusionSpawnableReferences.AchievementBoardReference,
                policyData = null,
            };

            AssetSpawner.Register(spawnable);

            SafeAssetSpawner.Spawn(spawnable, position, rotation);
        }
    }
}