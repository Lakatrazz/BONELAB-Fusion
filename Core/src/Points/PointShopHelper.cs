using BoneLib;

using SLZ.UI;

using UnityEngine;

using LabFusion.Utilities;

namespace LabFusion.Points {
    public static class PointShopHelper {
        public static void SetupPointShop(Vector3 position, Quaternion rotation, Vector3 scale) {
            // Create the GameObject
            GameObject shop = GameObject.Instantiate(FusionBundleLoader.PointShopPrefab);
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
