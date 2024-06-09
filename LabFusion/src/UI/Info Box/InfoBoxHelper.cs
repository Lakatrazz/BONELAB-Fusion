using UnityEngine;

using LabFusion.Utilities;

using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;

namespace LabFusion.UI
{
    public static class InfoBoxHelper
    {
        public const string InfoBoardBarcode = "Lakatrazz.FusionContent.Spawnable.InfoBoard";

        public static void CompleteInfoBoard(GameObject gameObject)
        {
            // Currently just needs to add the InfoBox script
            gameObject.AddComponent<InfoBox>();
        }

        public static void SpawnInfoBoard(Vector3 position, Quaternion rotation)
        {
            var spawnable = new Spawnable()
            {
                crateRef = new SpawnableCrateReference(InfoBoardBarcode),
                policyData = null,
            };

            AssetSpawner.Register(spawnable);

            AssetSpawner.Spawn(spawnable, position, rotation, new Il2CppSystem.Nullable<Vector3>(Vector3.one), true, new Il2CppSystem.Nullable<int>(0), null, null);
        }
    }
}