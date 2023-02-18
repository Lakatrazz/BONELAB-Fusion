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
        public static T LoadPersistentAsset<T>(this AssetBundle bundle, string name) where T : UnityEngine.Object {
            var asset = bundle.LoadAsset(name);

            if (asset != null) {
                asset.hideFlags = HideFlags.DontUnloadUnusedAsset;
                return asset.TryCast<T>();
            }

            return null;
        }

        public static AssetBundle LoadAssetBundle(string name) {
            // Android
            if (HelperMethods.IsAndroid()) {
                return EmbeddedAssetBundle.LoadFromAssembly(FusionMod.FusionAssembly, ResourcePaths.AndroidBundlePrefix + name);
            }
            // Windows
            else {
                return EmbeddedAssetBundle.LoadFromAssembly(FusionMod.FusionAssembly, ResourcePaths.WindowsBundlePrefix + name);
            }
        }

        public static void OnBundleLoad() {
            FusionContentLoader.OnBundleLoad();
            FusionPointItemLoader.OnBundleLoad();
        }

        public static void OnBundleUnloaded() {
            FusionContentLoader.OnBundleUnloaded();
            FusionPointItemLoader.OnBundleUnloaded();
        }
    }
}
