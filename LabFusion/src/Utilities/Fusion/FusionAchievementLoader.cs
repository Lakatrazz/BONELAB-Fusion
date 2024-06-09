using LabFusion.Data;
using LabFusion.SDK.Achievements;
using UnityEngine;

namespace LabFusion.Utilities
{
    public class AchievementPair
    {
        public WeakAssetReference<Texture2D> Preview { get; private set; } = new();

        public static AchievementPair LoadFromBundle(AssetBundle bundle, string name)
        {
            var pair = new AchievementPair();

            bundle.LoadPersistentAssetAsync<Texture2D>(ResourcePaths.PreviewPrefix + name, (v) => { pair.Preview.SetAsset(v); });

            return pair;
        }
    }

    public static class FusionAchievementLoader
    {
        public static WeakAssetReference<AssetBundle> AchievementBundle { get; private set; } = new();

        private static readonly string[] _achievementNames = new string[] {
            // Deathmatch
            nameof(ExperiencedExecutioner),
            nameof(MediocreMarksman),
            nameof(RookieAssassin),
            nameof(Rampage),

            // Bitmart
            nameof(StyleAddict),
            nameof(StyleBaby),
            nameof(StyleMan),
            
            // Campaign
            nameof(Betrayal),
            nameof(OneMoreTime),

            // Server
            nameof(HeadOfHouse),
            nameof(WarmWelcome),

            // Misc
            nameof(BouncingStrong),
            nameof(LavaGang),
            nameof(CleanupCrew),
            nameof(ClassStruggle),
            nameof(GuardianAngel),
            nameof(HighwayMan),
            nameof(DaycareAttendant),
            nameof(AroundTheWorld),
            nameof(HelloThere),
        };

        private static readonly Dictionary<string, AchievementPair> _achievementPairs = new();

        private static AssetBundleCreateRequest _achievementBundleRequest = null;

        private static void OnBundleCompleted(AsyncOperation operation)
        {
            var bundle = _achievementBundleRequest.assetBundle;
            AchievementBundle.SetAsset(bundle);

            foreach (var achievement in _achievementNames)
            {
                _achievementPairs.Add(achievement, AchievementPair.LoadFromBundle(bundle, achievement));
            }
        }

        public static void OnBundleLoad()
        {
            _achievementBundleRequest = FusionBundleLoader.LoadAssetBundleAsync(ResourcePaths.AchievementBundle);

            if (_achievementBundleRequest != null)
            {
                _achievementBundleRequest.add_completed((Il2CppSystem.Action<AsyncOperation>)OnBundleCompleted);
            }
            else
                FusionLogger.Error("Achievement bundle failed to load!");
        }

        public static void OnBundleUnloaded()
        {
            // Unload item bundle
            if (AchievementBundle.HasAsset)
            {
                AchievementBundle.Asset.Unload(true);
                AchievementBundle.UnloadAsset();
            }
        }

        public static AchievementPair GetPair(string name)
        {
            return _achievementPairs[name];
        }
    }
}