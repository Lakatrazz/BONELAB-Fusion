using LabFusion.Utilities;
using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class BritishHelm : AccessoryItem
    {
        public override string Title => "British Helm";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "Not very good at protecting innit.";

        public override int Price => 400;

        public override RarityLevel Rarity => RarityLevel.Blue;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(BritishHelm)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(BritishHelm)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Military",
        };
    }
}