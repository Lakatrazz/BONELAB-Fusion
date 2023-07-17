using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class PulsatingMass : AccessoryItem {
        public override string Title => "Pulsating Mass";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "A strangely pungent hat for your head...";

        public override int Price => 1700;

        public override RarityLevel Rarity => RarityLevel.Pink;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(PulsatingMass)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(PulsatingMass)).GameObject;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Mask",
            "Cosmetic",
            "Cursed",
        };
    }
}