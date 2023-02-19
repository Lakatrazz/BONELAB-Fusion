using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class BitsTrail : AccessoryItem {
        public override string Title => "LODS OF BITS";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "A true show of wealth! the coins are falling right out of your pockets";

        public override int Price => 5000;

        public override RarityLevel Rarity => RarityLevel.Red;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(BitsTrail)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(BitsTrail)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HIPS;

        public override string[] Tags => new string[3] {
            "Trail",
            "Cosmetic",
            "Valuable",
        };
    }
}