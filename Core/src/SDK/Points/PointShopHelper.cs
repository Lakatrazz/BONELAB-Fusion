using BoneLib;

using UnityEngine;

using LabFusion.Utilities;

using SLZ.Marrow.Warehouse;

namespace LabFusion.SDK.Points
{
    public static class PointShopHelper
    {
        public static void SetupPointShop(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            // Create the GameObject
            LevelCrate level = FusionSceneManager.Level;

            FusionContentLoader.PointShopPrefab.Load((go) =>
            {
                // Make sure the level hasn't changed
                if (level != FusionSceneManager.Level)
                    return;

                GameObject shop = GameObject.Instantiate(go);
                shop.SetActive(false);
                shop.transform.position = position;
                shop.transform.rotation = rotation;
                shop.transform.localScale = scale;
                shop.SetActive(true);

                // Add the point shop script
                shop.gameObject.AddComponent<PointShop>();
            });
        }
    }
}