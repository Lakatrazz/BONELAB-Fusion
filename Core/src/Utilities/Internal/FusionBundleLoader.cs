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

        public static Texture2D SabrelakeLogo { get; private set; }
        public static Texture2D LavaGangLogo { get; private set; }

        public static AudioClip SyntheticCavernsRemix { get; private set; }
        public static AudioClip WWWWonderLan { get; private set; }

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
                SabrelakeLogo = ContentBundle.LoadAsset(ResourcePaths.SabrelakeLogo).TryCast<Texture2D>(); 
                LavaGangLogo = ContentBundle.LoadAsset(ResourcePaths.LavaGangLogo).TryCast<Texture2D>();
                SyntheticCavernsRemix = ContentBundle.LoadAsset(ResourcePaths.SyntheticCavernsRemix).TryCast<AudioClip>();
                WWWWonderLan = ContentBundle.LoadAsset(ResourcePaths.WWWWonderLan).TryCast<AudioClip>();

                SabrelakeLogo.hideFlags = HideFlags.DontUnloadUnusedAsset;
                LavaGangLogo.hideFlags = HideFlags.DontUnloadUnusedAsset;
                SyntheticCavernsRemix.hideFlags = HideFlags.DontUnloadUnusedAsset;
                WWWWonderLan.hideFlags = HideFlags.DontUnloadUnusedAsset;
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
