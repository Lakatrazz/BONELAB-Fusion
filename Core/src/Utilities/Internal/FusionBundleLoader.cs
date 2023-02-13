using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BoneLib;

using LabFusion.Data;

using UnityEngine;

namespace LabFusion.Utilities {
    internal static class FusionBundleLoader {
        // Content bundle
        public static AssetBundle ContentBundle { get; private set; }

        public static Sprite SabrelakeLogo { get; private set; }
        public static Sprite LavaGangLogo { get; private set; }

        public static void OnBundleLoad() {
            // Load content bundle
            // Get the correct assetbundle
            // Android
            if (HelperMethods.IsAndroid()) {
                ContentBundle = EmbeddedAssetBundle.LoadFromAssembly(FusionMod.FusionAssembly, ResourcePaths.AndroidBundlePrefix + ResourcePaths.ContentBundle);
            }
            // Windows
            else {
                ContentBundle = EmbeddedAssetBundle.LoadFromAssembly(FusionMod.FusionAssembly, ResourcePaths.WindowsBundlePrefix + ResourcePaths.ContentBundle);
            }

            if (ContentBundle != null) {
                SabrelakeLogo = (Sprite)ContentBundle.Load<Sprite>(ResourcePaths.SabrelakeLogo);
                LavaGangLogo = (Sprite)ContentBundle.Load<Sprite>(ResourcePaths.LavaGangLogo);
            }
            else
                FusionLogger.Error("Content Bundle failed to load!");
        }

        public static void OnBundleUnloaded() {
            // Unload content bundle
            if (ContentBundle != null)
                ContentBundle.Unload(true);
        }
    }
}
