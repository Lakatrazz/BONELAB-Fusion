using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class VioletVortex : AccessoryItem
    {
        public override string Title => "Violet Vortex";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "All consuming.";

        public override int Price => 5000;

        public override RarityLevel Rarity => RarityLevel.Red;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(VioletVortex)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(VioletVortex)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Effect",
            "Cosmetic",
            "Swirly",
        };
    }
}