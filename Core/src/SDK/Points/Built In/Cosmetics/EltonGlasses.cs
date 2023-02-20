using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class EltonGlasses : AccessoryItem {
        public override string Title => "Circle Shades";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "Stylish! and circular.";

        public override int Price => 500;

        public override RarityLevel Rarity => RarityLevel.LightRed;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(EltonGlasses)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(EltonGlasses)).GameObject;

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