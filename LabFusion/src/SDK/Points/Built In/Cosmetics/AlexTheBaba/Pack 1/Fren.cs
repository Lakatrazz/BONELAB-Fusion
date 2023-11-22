using LabFusion.Utilities;
using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class Fren : AccessoryItem
    {
        public override string Title => "Fren";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "Straight outta fren world.";

        public override int Price => 1000;

        public override RarityLevel Rarity => RarityLevel.Green;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(Fren)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(Fren)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Creature",
        };
    }
}