using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class CatBeanie : AccessoryItem
    {
        public override string Title => "Cat Beanie";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "A nice little hat for your head ";

        public override int Price => 600;

        public override RarityLevel Rarity => RarityLevel.Orange;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(CatBeanie)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(CatBeanie)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Cute",
        };
    }
}