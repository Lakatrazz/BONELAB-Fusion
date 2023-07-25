using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class OctoSpecs : AccessoryItem {
        public override string Title => "Octo Specs";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "My enemies call me Mr Octagon.";

        public override int Price => 700;

        public override RarityLevel Rarity => RarityLevel.Blue;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(OctoSpecs)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(OctoSpecs)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.EYE_CENTER;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Glasses",
            "Cosmetic",
            "Villain",
        };
    }
}