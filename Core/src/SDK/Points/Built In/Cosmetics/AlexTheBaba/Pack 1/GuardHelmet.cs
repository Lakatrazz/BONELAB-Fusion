using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class GuardHelmet : AccessoryItem {
        public override string Title => "Guard Helmet";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "Beer not included.";

        public override int Price => 500;

        public override RarityLevel Rarity => RarityLevel.Lime;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(GuardHelmet)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(GuardHelmet)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Military",
        };
    }
}