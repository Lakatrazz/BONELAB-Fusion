using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class HotHead : AccessoryItem
    {
        public override string Title => "Hot Head";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "Smokin'";

        public override int Price => 4500;

        public override RarityLevel Rarity => RarityLevel.Red;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(HotHead)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(HotHead)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Effect",
            "Cosmetic",
            "Hot",
        };
    }
}