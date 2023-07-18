using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class PieceOfResistance : AccessoryItem {
        public override string Title => "Plastic Piece";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "May come of use to someone special. too bad its already stuck on your back.";

        public override int Price => 400;

        public override RarityLevel Rarity => RarityLevel.White;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(PieceOfResistance)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(PieceOfResistance)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.CHEST_BACK;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEIGHT;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Backpack",
            "Cosmetic",
            "Hero",
        };
    }
}