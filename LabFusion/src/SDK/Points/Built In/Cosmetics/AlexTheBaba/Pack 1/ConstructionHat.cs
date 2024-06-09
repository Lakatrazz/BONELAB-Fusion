using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class ConstructionHat : AccessoryItem
    {
        public override string Title => "Construction Helm";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "Can we fix it?";

        public override int Price => 300;

        public override RarityLevel Rarity => RarityLevel.Blue;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(ConstructionHat)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(ConstructionHat)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Job",
        };
    }
}