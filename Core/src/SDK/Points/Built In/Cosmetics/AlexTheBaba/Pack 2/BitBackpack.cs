using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class BitBackpack : AccessoryItem
    {
        public override string Title => "Big Bit Backpack";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "Filled with lots and lots of little bits!";

        public override int Price => 7400;

        public override RarityLevel Rarity => RarityLevel.Red;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(BitBackpack)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(BitBackpack)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.CHEST_BACK;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEIGHT;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Backpack",
            "Cosmetic",
            "Valuable",
        };
    }
}