using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class GloopTrail : AccessoryItem {
        public override string Title => "Gloop Trail";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "An extra gloopy trail to go with the hat!";

        public override int Price => 1200;

        public override RarityLevel Rarity => RarityLevel.LightRed;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(GloopTrail)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(GloopTrail)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HIPS;

        public override string[] Tags => new string[3] {
            "Trail",
            "Cosmetic",
            "Junk",
        };
    }
}