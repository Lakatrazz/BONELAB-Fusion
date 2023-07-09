using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BoneLib;

using LabFusion.Data;
using LabFusion.SDK.Achievements;
using UnityEngine;

namespace LabFusion.Utilities {
    public class AchievementPair {
        public Texture2D Preview { get; private set; }

        public static AchievementPair LoadFromBundle(AssetBundle bundle, string name) {
            return new AchievementPair() {
                Preview = bundle.LoadPersistentAsset<Texture2D>(ResourcePaths.PreviewPrefix + name),
            };
        }
    }

    public static class FusionAchievementLoader {
        public static AssetBundle AchievementBundle { get; private set; }

        private static readonly string[] _achievementNames = new string[] {
            nameof(HeadOfHouse),
            nameof(ExperiencedExecutioner),
            nameof(MediocreMarksman),
            nameof(RookieAssassin),
            nameof(WarmWelcome)
        };

        private static readonly Dictionary<string, AchievementPair> _achievementPairs = new();

        public static void OnBundleLoad() {
            AchievementBundle = FusionBundleLoader.LoadAssetBundle(ResourcePaths.AchievementBundle);

            if (AchievementBundle != null) {
                foreach (var achievement in _achievementNames) {
                    _achievementPairs.Add(achievement, AchievementPair.LoadFromBundle(AchievementBundle, achievement));
                }
            }
            else
                FusionLogger.Error("Achievement bundle failed to load!");
        }

        public static void OnBundleUnloaded() {
            // Unload item bundle
            if (AchievementBundle != null)
                AchievementBundle.Unload(true);
        }

        public static AchievementPair GetPair(string name) {
            return _achievementPairs[name];
        }
    }
}
