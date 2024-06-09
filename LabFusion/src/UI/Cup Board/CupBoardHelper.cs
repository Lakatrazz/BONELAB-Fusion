using UnityEngine;

using LabFusion.Utilities;

using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.UI
{
    public static class CupBoardHelper
    {
        public static void SetupCupBoard(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            // Create the GameObject
            LevelCrate level = FusionSceneManager.Level;

            FusionContentLoader.CupBoardPrefab.Load((go) =>
            {
                // Make sure the level hasn't changed
                if (level != FusionSceneManager.Level)
                    return;

                GameObject board = GameObject.Instantiate(go);
                board.SetActive(false);
                board.transform.position = position;
                board.transform.rotation = rotation;
                board.transform.localScale = scale;
                board.SetActive(true);

                // Add the cup board script
                board.gameObject.AddComponent<CupBoard>();
            });
        }
    }
}