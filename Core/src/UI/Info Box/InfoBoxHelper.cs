using UnityEngine;

using LabFusion.Utilities;

namespace LabFusion.UI {
    public static class InfoBoxHelper {
        public static void SetupInfoBox(Vector3 position, Quaternion rotation, Vector3 scale) {
            // Make sure we have the prefab
            if (FusionContentLoader.InfoBoxPrefab == null) {
                FusionLogger.Warn("Missing the Info Box prefab!");
                return;
            }

            // Create the GameObject
            GameObject shop = GameObject.Instantiate(FusionContentLoader.InfoBoxPrefab);
            shop.SetActive(false);
            shop.transform.position = position;
            shop.transform.rotation = rotation;
            shop.transform.localScale = scale;
            shop.SetActive(true);

            // Add the info box script
            shop.gameObject.AddComponent<InfoBox>();
        }
    }
}
