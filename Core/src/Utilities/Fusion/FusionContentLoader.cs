using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BoneLib;

using LabFusion.Data;

using UnityEngine;

namespace LabFusion.Utilities {
    public static class FusionContentLoader {
        public static AssetBundle ContentBundle { get; private set; }

        public static GameObject PointShopPrefab { get; private set; }
        public static GameObject InfoBoxPrefab { get; private set; }
        public static GameObject CupBoardPrefab { get; private set; }

        public static GameObject EntangledLinePrefab { get; private set; }

        public static GameObject AchievementPopupPrefab { get; private set; }
        public static GameObject BitPopupPrefab { get; private set; }

        public static Texture2D SabrelakeLogo { get; private set; }
        public static Texture2D LavaGangLogo { get; private set; }

        public static AudioClip GeoGrpFellDownTheStairs { get; private set; }
        public static AudioClip BouncingStrong { get; private set; }

        public static AudioClip LavaGangVictory { get; private set; }
        public static AudioClip SabrelakeVictory { get; private set; }

        public static AudioClip LavaGangFailure { get; private set; }
        public static AudioClip SabrelakeFailure { get; private set; }

        public static AudioClip DMTie { get; private set; }

        public static AudioClip BitGet { get; private set; }

        public static AudioClip UISelect { get; private set; }
        public static AudioClip UIDeny { get; private set; }
        public static AudioClip UIConfirm { get; private set; }
        public static AudioClip UITurnOff { get; private set; }
        public static AudioClip UITurnOn { get; private set; }

        public static AudioClip PurchaseFailure { get; private set; }
        public static AudioClip PurchaseSuccess { get; private set; }

        public static AudioClip EquipItem { get; private set; }
        public static AudioClip UnequipItem { get; private set; }

        public static Texture2D NotificationInformation { get; private set; }
        public static Texture2D NotificationWarning { get; private set; }
        public static Texture2D NotificationError { get; private set; }
        public static Texture2D NotificationSuccess { get; private set; }

        // Laser cursor
        public static GameObject LaserCursor { get; private set; }
        public static AudioClip LaserPulseSound { get; private set; }
        public static AudioClip LaserRaySpawn { get; private set; }
        public static AudioClip LaserRayDespawn { get; private set; }
        public static AudioClip LaserPrismaticSFX { get; private set; }

        private static readonly string[] _combatSongNames = new string[6] {
            "music_FreqCreepInModulationBuggyPhysics",
            "music_SicklyBugInitiative",
            "music_SyntheticCavernsRemix",
            "music_WWWonderlan",
            "music_SmigglesInDespair",
            "music_AppenBeyuge",
        };

        private static readonly List<AudioClip> _combatPlaylist = new List<AudioClip>();
        public static AudioClip[] CombatPlaylist => _combatPlaylist.ToArray();

        public static void OnBundleLoad() {
            ContentBundle = FusionBundleLoader.LoadAssetBundle(ResourcePaths.ContentBundle);

            if (ContentBundle != null) {
                PointShopPrefab = ContentBundle.LoadPersistentAsset<GameObject>(ResourcePaths.PointShopPrefab);
                InfoBoxPrefab = ContentBundle.LoadPersistentAsset<GameObject>(ResourcePaths.InfoBoxPrefab);
                CupBoardPrefab = ContentBundle.LoadPersistentAsset<GameObject>(ResourcePaths.CupBoardPrefab);

                EntangledLinePrefab = ContentBundle.LoadPersistentAsset<GameObject>(ResourcePaths.EntangledLinePrefab);

                AchievementPopupPrefab = ContentBundle.LoadPersistentAsset<GameObject>(ResourcePaths.AchievementPopupPrefab);
                BitPopupPrefab = ContentBundle.LoadPersistentAsset<GameObject>(ResourcePaths.BitPopupPrefab);

                SabrelakeLogo = ContentBundle.LoadPersistentAsset<Texture2D>(ResourcePaths.SabrelakeLogo); 
                LavaGangLogo = ContentBundle.LoadPersistentAsset<Texture2D>(ResourcePaths.LavaGangLogo);

                foreach (var song in _combatSongNames) {
                    _combatPlaylist.Add(ContentBundle.LoadPersistentAsset<AudioClip>(song));
                }

                GeoGrpFellDownTheStairs = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.GeoGrpFellDownTheStairs);
                BouncingStrong = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.BouncingStrong);

                LavaGangVictory = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.LavaGangVictory);
                SabrelakeVictory = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.SabrelakeVictory);

                LavaGangFailure = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.LavaGangFailure);
                SabrelakeFailure = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.SabrelakeFailure);

                DMTie = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.DMTie);

                BitGet = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.BitGet);

                UISelect = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.UISelect);
                UIDeny = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.UIDeny);
                UIConfirm = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.UIConfirm);
                UITurnOff = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.UITurnOff);
                UITurnOn = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.UITurnOn);

                PurchaseFailure = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.PurchaseFailure);
                PurchaseSuccess = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.PurchaseSuccess);

                EquipItem = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.EquipItem);
                UnequipItem = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.UnequipItem);

                NotificationInformation = ContentBundle.LoadPersistentAsset<Texture2D>(ResourcePaths.NotificationInformation);
                NotificationWarning = ContentBundle.LoadPersistentAsset<Texture2D>(ResourcePaths.NotificationWarning);
                NotificationError = ContentBundle.LoadPersistentAsset<Texture2D>(ResourcePaths.NotificationError);
                NotificationSuccess = ContentBundle.LoadPersistentAsset<Texture2D>(ResourcePaths.NotificationSuccess);

                LaserCursor = ContentBundle.LoadPersistentAsset<GameObject>(ResourcePaths.LaserCursor);
                LaserPulseSound = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.LaserPulseSound);
                LaserRaySpawn = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.LaserRaySpawn);
                LaserRayDespawn = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.LaserRayDespawn);
                LaserPrismaticSFX = ContentBundle.LoadPersistentAsset<AudioClip>(ResourcePaths.LaserPrismaticSFX);
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
