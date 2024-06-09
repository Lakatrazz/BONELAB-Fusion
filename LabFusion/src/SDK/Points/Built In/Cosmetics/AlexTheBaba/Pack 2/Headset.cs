using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class Headset : AccessoryItem
    {
        public override string Title => "Headset";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "Perfect for the bat-wielding mercenary in us all.";

        public override int Price => 500;

        public override RarityLevel Rarity => RarityLevel.Blue;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(Headset)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(Headset)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Gear",
        };
    }
}