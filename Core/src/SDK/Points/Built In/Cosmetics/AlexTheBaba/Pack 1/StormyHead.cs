using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class StormyHead : AccessoryItem {
        public override string Title => "Stormy Head Effect";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "For when you're feeling ever so gloomy.";

        public override int Price => 2200;

        public override RarityLevel Rarity => RarityLevel.Cyan;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(StormyHead)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(StormyHead)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Effect",
            "Cosmetic",
            "Depressing",
        };
    }
}