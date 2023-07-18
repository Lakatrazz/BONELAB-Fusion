using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class Gemstone : AccessoryItem {
        public override string Title => "Ancient Artifact";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "From a deep dark cavern...";

        public override int Price => 1500;

        public override RarityLevel Rarity => RarityLevel.Orange;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(Gemstone)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(Gemstone)).GameObject;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Valuable",
        };
    }
}