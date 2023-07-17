using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class Fedoral : AccessoryItem {
        public override string Title => "Fedoral";

        public override string Author => BitEconomy.RiggleAuthor;

        public override string Description => "Thanks for the gold, kind stranger";

        public override int Price => 60;

        public override RarityLevel Rarity => RarityLevel.Gray;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(Fedoral)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(Fedoral)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Sweaty",
        };
    }
}