using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class Junktion : AccessoryItem {
        public override string Title => "Junk-tion";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "A place to store all the bloat! ready for transportation via flushed recycling co.";

        public override int Price => 200;

        public override RarityLevel Rarity => RarityLevel.Gray;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(Junktion)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(Junktion)).GameObject;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Junk",
        };
    }
}