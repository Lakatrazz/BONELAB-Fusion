using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class WaistRing : AccessoryItem {
        public override string Title => "Purple-Green Aura";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "A mythical aura to surround your waist";

        public override int Price => 3200;

        public override RarityLevel Rarity => RarityLevel.Red;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(WaistRing)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(WaistRing)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HIPS;

        public override string[] Tags => new string[3] {
            "Ring",
            "Cosmetic",
            "Mythical",
        };
    }
}