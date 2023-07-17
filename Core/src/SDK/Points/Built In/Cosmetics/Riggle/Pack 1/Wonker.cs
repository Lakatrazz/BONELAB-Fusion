using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class Wonker : AccessoryItem {
        public override string Title => "Wonker";

        public override string Author => BitEconomy.RiggleAuthor;

        public override string Description => "Belonged to Wacky Willy's long lost cousin.";

        public override int Price => 90;

        public override RarityLevel Rarity => RarityLevel.White;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(Wonker)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(Wonker)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Crazy",
        };
    }
}