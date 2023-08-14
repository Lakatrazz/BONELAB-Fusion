using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class LightningHead : AccessoryItem {
        public override string Title => "Shocking Head Effect";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "BZzzzzbBZzzzz ZZZzz ZZ (the sound your head is making)";

        public override int Price => 3000;

        public override RarityLevel Rarity => RarityLevel.Red;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(LightningHead)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(LightningHead)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Effect",
            "Cosmetic",
            "Hazard",
        };
    }
}