using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class WeirdShades : AccessoryItem {
        public override string Title => "Shiny Shades";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "Dont just block the sun. redirect it towards everyone around you.";

        public override int Price => 600;

        public override RarityLevel Rarity => RarityLevel.Lime;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(WeirdShades)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(WeirdShades)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.NOSE;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Eyewear",
            "Cosmetic",
            "Stylish",
        };
    }
}