using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class SorcererHat : AccessoryItem {
        public override string Title => "Magicians Hat";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "Mops not included.";

        public override int Price => 500;

        public override RarityLevel Rarity => RarityLevel.Green;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(SorcererHat)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(SorcererHat)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Magic",
        };
    }
}