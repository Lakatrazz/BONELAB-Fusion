using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class CardboardTophat : AccessoryItem {
        public override string Title => "Cardboard Tophat";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "Found somewhere between a post office and a clothes store. that somewhere being the dumpster. BUT ISNT IT JUST SO FASIONABLE!?";

        public override int Price => 600;

        public override RarityLevel Rarity => RarityLevel.White;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(CardboardTophat)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(CardboardTophat)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Cardboard",
        };
    }
}