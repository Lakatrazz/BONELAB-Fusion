using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class NyanTrail : AccessoryItem {
        public override string Title => "Nyan Trail";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "nyaynyanmewmew, do you feel filled with strawberries? or rainbows?";

        public override int Price => 2010;

        public override RarityLevel Rarity => RarityLevel.Pink;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(NyanTrail)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(NyanTrail)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HIPS;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override string[] Tags => new string[4] {
            "Trail",
            "Cosmetic",
            "Rainbow",
            "Nostalgic",
        };
    }
}