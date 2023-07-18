using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class MissedAWideGlasses : AccessoryItem {
        public override string Title => "Missed A Wide Glasses";

        public override string Author => BitEconomy.RiggleAuthor;

        public override string Description => "Made of granite";

        public override int Price => 1200;

        public override RarityLevel Rarity => RarityLevel.Cyan;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(MissedAWideGlasses)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(MissedAWideGlasses)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.EYE_CENTER;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Illegal",
        };
    }
}