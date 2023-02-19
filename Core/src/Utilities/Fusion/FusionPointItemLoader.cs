using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BoneLib;

using LabFusion.Data;
using LabFusion.SDK.Points;

using UnityEngine;

namespace LabFusion.Utilities {
    internal class ItemPair {
        public GameObject GameObject { get; private set; }
        public Texture2D Preview { get; private set; }

        public static ItemPair LoadFromBundle(AssetBundle bundle, string name) {
            return new ItemPair() {
                GameObject = bundle.LoadPersistentAsset<GameObject>(ResourcePaths.ItemPrefix + name),
                Preview = bundle.LoadPersistentAsset<Texture2D>(ResourcePaths.PreviewPrefix + name),
            };
        }
    }

    internal static class FusionPointItemLoader {
        public static AssetBundle ItemBundle { get; private set; }

        private static readonly string[] _itemNames = new string[39] {
            // BaBa Corp Cosmetics
            nameof(Gemstone),
            nameof(GloopTrail),
            nameof(Junktion),
            nameof(PulsatingMass),
            nameof(RubixCube),
            nameof(BitsTrail),
            nameof(CardboardTophat),
            nameof(Glasses3D),
            nameof(AdventureHat),
            nameof(WaistRing),
            nameof(BodyPillow),
            nameof(BritishHelm),
            nameof(CardboardDisguise),
            nameof(CatBeanie),
            nameof(CheeseHat),
            nameof(ConstructionHat),
            nameof(Cooler),
            nameof(EltonGlasses),
            nameof(EnaHat),
            nameof(Fez),
            nameof(Firework),
            nameof(Fren),
            nameof(FryCookHat),
            nameof(Gearhead),
            nameof(GuardHelmet),
            nameof(KickMeSign),
            nameof(KnollHat),
            nameof(LaytonHat),
            nameof(LegoHead),
            nameof(LightningHead),
            nameof(Monocle),
            nameof(NyanTrail),
            nameof(PartyHat),
            nameof(PieceOfResistance),
            nameof(SorcererHat),
            nameof(StormyHead),
            nameof(TestItem),
            nameof(TheBeacon),
            nameof(WeirdShades),
        };

        private static readonly Dictionary<string, ItemPair> _itemPairs = new Dictionary<string, ItemPair>();

        public static void OnBundleLoad() {
            ItemBundle = FusionBundleLoader.LoadAssetBundle(ResourcePaths.ItemBundle);

            if (ItemBundle != null) {
                foreach (var item in _itemNames) {
                    _itemPairs.Add(item, ItemPair.LoadFromBundle(ItemBundle, item));
                }
            }
            else
                FusionLogger.Error("Item bundle failed to load!");
        }

        public static void OnBundleUnloaded() {
            // Unload item bundle
            if (ItemBundle != null)
                ItemBundle.Unload(true);
        }

        public static ItemPair GetPair(string name) {
            return _itemPairs[name];
        }
    }
}
