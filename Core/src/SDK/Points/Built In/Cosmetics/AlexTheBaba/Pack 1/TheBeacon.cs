using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class TheBeacon : AccessoryItem {
        public override string Title => "The Beacon";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "For when you really wanna be the target. (comes with pulse particle)";

        public override int Price => 2000;

        public override RarityLevel Rarity => RarityLevel.Red;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(TheBeacon)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(TheBeacon)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Alien",
        };
    }
}