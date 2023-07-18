using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class Firework : AccessoryItem {
        public override string Title => "Firework Backpack";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "TO INFINITY!";

        public override int Price => 1100;

        public override RarityLevel Rarity => RarityLevel.Lime;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(Firework)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(Firework)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.CHEST_BACK;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEIGHT;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Backpack",
            "Cosmetic",
            "Explosive",
        };
    }
}