using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class Crown : AccessoryItem
    {
        public override string Title => "Crown";

        public override string Author => BitEconomy.RiggleAuthor;

        public override string Description => "New king!";

        public override int Price => 600;

        public override RarityLevel Rarity => RarityLevel.White;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(Crown)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(Crown)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Rich",
        };
    }
}