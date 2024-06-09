using UnityEngine;

using LabFusion.Utilities;

using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.UI
{
    public static class InfoBoxHelper
    {
        public static void SetupInfoBox(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            // Create the GameObject
            LevelCrate level = FusionSceneManager.Level;

            FusionContentLoader.InfoBoxPrefab.Load((go) =>
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

                // Add the info box script
                shop.gameObject.AddComponent<InfoBox>();
            });
        }
    }
}