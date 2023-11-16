using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class Floatie : AccessoryItem
    {
        public override string Title => "Floatie";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "Approximate weight of 240 KG";

        public override int Price => 2400;

        public override RarityLevel Rarity => RarityLevel.Pink;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(Floatie)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(Floatie)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HIPS;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEIGHT;

        public override bool IsHiddenInView => false;

        public override string[] Tags => new string[3] {
            "Ring",
            "Cosmetic",
            "Pool",
        };
    }
}