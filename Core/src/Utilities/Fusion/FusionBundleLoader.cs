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

        public static GameObject PointShopPrefab { get; private set; }

        public static Texture2D SabrelakeLogo { get; private set; }
        public static Texture2D LavaGangLogo { get; private set; }

        public static AudioClip SyntheticCavernsRemix { get; private set; }
        public static AudioClip WWWWonderLan { get; private set; }
        public static AudioClip SicklyBugInitiative { get; private set; }

        public static AudioClip LavaGangVictory { get; private set; }
        public static AudioClip SabrelakeVictory { get; private set; }

        public static AudioClip LavaGangFailure { get; private set; }
        public static AudioClip SabrelakeFailure { get; private set; }

        public static AudioClip UISelect { get; private set; }
        public static AudioClip UIDeny { get; private set; }
        public static AudioClip UIConfirm { get; private set; }

        public static AudioClip PurchaseFailure { get; private set; }
        public static AudioClip PurchaseSuccess { get; private set; }

        public static AudioClip EquipItem { get; private set; }

        public static AudioClip[] CombatPlaylist => new AudioClip[3]
        {
            SyntheticCavernsRemix,
            WWWWonderLan,
            SicklyBugInitiative
        };

        private static T LoadPersistentAsset<T>(this AssetBundle bundle, string name) where T : UnityEngine.Object {
            var asset = bundle.LoadAsset(name);

            if (asset != null) {
                asset.hideFlags = HideFlags.DontUnloadUnusedAsset;
                return asset.TryCast<T>();
            }

            return null;
        }

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
                PointShopPrefab = ContentBundle.LoadPersistentAsset<GameObject>(ResourcePaths.PointShopPrefab);

                SabrelakeLogo = ContentBundle.LoadPersistentAsset<Texture2D>(ResourcePaths.SabrelakeLogo); 
                LavaGangLogo = ContentBundle.LoadPersistentAsset<Texture2D>(ResourcePaths.LavaGangLogo);

                SyntheticCavernsRemix = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.SyntheticCavernsRemix);
                WWWWonderLan = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.WWWWonderLan);
                SicklyBugInitiative = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.SicklyBugInitiative);

                LavaGangVictory = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.LavaGangVictory);
                SabrelakeVictory = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.SabrelakeVictory);

                LavaGangFailure = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.LavaGangFailure);
                SabrelakeFailure = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.SabrelakeFailure);

                UISelect = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.UISelect);
                UIDeny = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.UIDeny);
                UIConfirm = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.UIConfirm);

                PurchaseFailure = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.PurchaseFailure);
                PurchaseSuccess = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.PurchaseSuccess);

                EquipItem = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.EquipItem);
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
