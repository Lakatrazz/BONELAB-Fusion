using UnityEngine;

using LabFusion.Marrow;
using LabFusion.Marrow.Pool;

namespace LabFusion.UI;

public static class CupBoardHelper
{
    public static void CompleteAchievementBoard(GameObject gameObject)
    {
        // Currently just needs to add the CupBoard script
        gameObject.AddComponent<CupBoard>();
    }

    public static void SpawnAchievementBoard(Vector3 position, Quaternion rotation)
    {
        var spawnable = LocalAssetSpawner.CreateSpawnable(FusionSpawnableReferences.AchievementBoardReference);

        LocalAssetSpawner.Register(spawnable);

        LocalAssetSpawner.Spawn(spawnable, position, rotation);
    }
}