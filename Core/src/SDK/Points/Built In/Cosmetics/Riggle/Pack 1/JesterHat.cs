using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class JesterHat : AccessoryItem {
        public override string Title => "The Jester";

        public override string Author => BitEconomy.RiggleAuthor;

        public override string Description => "Josh's jingle";

        public override int Price => 500;

        public override RarityLevel Rarity => RarityLevel.Green;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(JesterHat)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(JesterHat)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Silly",
        };
    }
}