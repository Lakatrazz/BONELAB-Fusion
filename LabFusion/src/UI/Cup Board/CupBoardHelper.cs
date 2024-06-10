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

            AssetSpawner.Spawn(spawnable, position, rotation, new Il2CppSystem.Nullable<Vector3>(Vector3.one), true, new Il2CppSystem.Nullable<int>(0), null, null);
        }
    }
}