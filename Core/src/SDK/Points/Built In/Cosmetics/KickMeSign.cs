using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class KickMeSign : AccessoryItem {
        public override string Title => "Kick Me Sign";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "Why would you even choose to wear this.";

        public override int Price => 100;

        public override RarityLevel Rarity => RarityLevel.Gray;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(KickMeSign)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(KickMeSign)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.CHEST_BACK;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Backpack",
            "Cosmetic",
            "Junk",
        };
    }
}