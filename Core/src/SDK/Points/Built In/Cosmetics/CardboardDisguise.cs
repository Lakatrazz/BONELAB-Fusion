using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class CardboardDisguise : AccessoryItem {
        public override string Title => "Cardboard Disguise";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "Get into silly club! by wearing this silly disguise!!!";

        public override int Price => 300;

        public override RarityLevel Rarity => RarityLevel.Gray;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(CardboardDisguise)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(CardboardDisguise)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Mask",
            "Cosmetic",
            "Junk",
        };
    }
}