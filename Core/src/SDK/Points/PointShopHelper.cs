using BoneLib;

using SLZ.UI;

using UnityEngine;

using LabFusion.Utilities;

namespace LabFusion.SDK.Points {
    public static class PointShopHelper {
        public static void SetupPointShop(Vector3 position, Quaternion rotation, Vector3 scale) {
            // Make sure we have the prefab
            if (FusionContentLoader.PointShopPrefab == null) {
                FusionLogger.Warn("Missing the Point Shop prefab!");
                return;
            }

            // Create the GameObject
            GameObject shop = GameObject.Instantiate(FusionContentLoader.PointShopPrefab);
            shop.SetActive(false);
            shop.transform.position = position;
            shop.transform.rotation = rotation;
            shop.transform.localScale = scale;
            shop.SetActive(true);

            // Add the point shop script
            shop.gameObject.AddComponent<PointShop>();
        }
    }
}
