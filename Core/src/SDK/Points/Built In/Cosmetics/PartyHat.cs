using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class PartyHat : AccessoryItem {
        public override string Title => "Party Hat";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "Be the life of the party! or not.";

        public override int Price => 100;

        public override RarityLevel Rarity => RarityLevel.Gray;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(PartyHat)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(PartyHat)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Birthday",
        };
    }
}