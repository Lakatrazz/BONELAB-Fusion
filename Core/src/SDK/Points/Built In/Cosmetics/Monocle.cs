using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class Monocle : AccessoryItem {
        public override string Title => "Monocle";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "Prim and proper i must say! Tally ho!";

        public override int Price => 100;

        public override RarityLevel Rarity => RarityLevel.Green;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(Monocle)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(Monocle)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.EYE_RIGHT;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Eyewear",
            "Cosmetic",
            "Exquisite",
        };
    }
}