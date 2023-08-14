using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class AncientTablet : AccessoryItem {
        public override string Title => "Ancient Tablet";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "The inscription says....";

        public override int Price => 1400;

        public override RarityLevel Rarity => RarityLevel.Green;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(AncientTablet)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(AncientTablet)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.CHEST_BACK;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEIGHT;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Backpack",
            "Cosmetic",
            "Mystery",
        };
    }
}