using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class ArrowHead : AccessoryItem
    {
        public override string Title => "Arrow Head";

        public override string Author => BitEconomy.RiggleAuthor;

        public override string Description => "OUCHIE MY HEAD!";

        public override int Price => 200;

        public override RarityLevel Rarity => RarityLevel.White;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(ArrowHead)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(ArrowHead)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Painful",
        };
    }
}