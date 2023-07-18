using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class Potion : AccessoryItem {
        public override string Title => "Potion Pack";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "My potions are too strong for you traveler.";

        public override int Price => 4000;

        public override RarityLevel Rarity => RarityLevel.Pink;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(Potion)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(Potion)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.CHEST_BACK;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEIGHT;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Backpack",
            "Cosmetic",
            "Tasty",
        };
    }
}