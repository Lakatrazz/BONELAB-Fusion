using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class BodyPillow : AccessoryItem {
        public override string Title => "Null Pillow";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "Wear the love of your life on your back <3.";

        public override int Price => 600;

        public override RarityLevel Rarity => RarityLevel.Green;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(BodyPillow)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(BodyPillow)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.CHEST_BACK;

        public override string[] Tags => new string[3] {
            "Backpack",
            "Cosmetic",
            "Soft",
        };
    }
}