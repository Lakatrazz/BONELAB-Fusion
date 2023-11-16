using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class ZestySwagShades : AccessoryItem
    {
        public override string Title => "Zesty Swag Shades";

        public override string Author => BitEconomy.RiggleAuthor;

        public override string Description => "Ayo?";

        public override int Price => 2900;

        public override RarityLevel Rarity => RarityLevel.Lime;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(ZestySwagShades)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(ZestySwagShades)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.EYE_CENTER;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Swag",
        };
    }
}