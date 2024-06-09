using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class BucketHat : AccessoryItem
    {
        public override string Title => "Bucket Hat";

        public override string Author => BitEconomy.RiggleAuthor;

        public override string Description => "The kinda bucky you put yo head in";

        public override int Price => 1000;

        public override RarityLevel Rarity => RarityLevel.LightRed;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(BucketHat)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(BucketHat)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Bucket",
        };
    }
}