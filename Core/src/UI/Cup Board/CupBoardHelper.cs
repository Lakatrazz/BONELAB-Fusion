using UnityEngine;

using LabFusion.Utilities;

namespace LabFusion.UI {
    public static class CupBoardHelper {
        public static void SetupCupBoard(Vector3 position, Quaternion rotation, Vector3 scale) {
            // Make sure we have the prefab
            if (FusionContentLoader.CupBoardPrefab == null) {
                FusionLogger.Warn("Missing the Cup Board prefab!");
                return;
            }

            // Create the GameObject
            GameObject board = GameObject.Instantiate(FusionContentLoader.CupBoardPrefab);
            board.SetActive(false);
            board.transform.position = position;
            board.transform.rotation = rotation;
            board.transform.localScale = scale;
            board.SetActive(true);

            // Add the cup board script
            board.gameObject.AddComponent<CupBoard>();
        }
    }
}
