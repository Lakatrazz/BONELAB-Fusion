using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class MissedAWideHat : AccessoryItem
    {
        public override string Title => "Missed A Wide Hat";

        public override string Author => BitEconomy.RiggleAuthor;

        public override string Description => "I did it for the nookie";

        public override int Price => 1200;

        public override RarityLevel Rarity => RarityLevel.Cyan;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(MissedAWideHat)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(MissedAWideHat)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Illegal",
        };
    }
}