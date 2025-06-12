using UnityEngine;

using LabFusion.Marrow;
using LabFusion.Marrow.Pool;

namespace LabFusion.UI;

public static class InfoBoxHelper
{
    public static void CompleteInfoBoard(GameObject gameObject)
    {
        // Currently just needs to add the InfoBox script
        gameObject.AddComponent<InfoBox>();
    }

    public static void SpawnInfoBoard(Vector3 position, Quaternion rotation)
    {
        var spawnable = LocalAssetSpawner.CreateSpawnable(FusionSpawnableReferences.InfoBoardReference);

        LocalAssetSpawner.Register(spawnable);

        LocalAssetSpawner.Spawn(spawnable, position, rotation);
    }
}