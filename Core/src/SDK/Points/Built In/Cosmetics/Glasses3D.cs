using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class Glasses3D : AccessoryItem {
        public override string Title => "3D Glasses";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "BONELAB! NOW IN 3D!";

        public override int Price => 300;

        public override RarityLevel Rarity => RarityLevel.Green;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(Glasses3D)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(Glasses3D)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.NOSE;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Eyewear",
            "Cosmetic",
            "Fun",
        };
    }
}