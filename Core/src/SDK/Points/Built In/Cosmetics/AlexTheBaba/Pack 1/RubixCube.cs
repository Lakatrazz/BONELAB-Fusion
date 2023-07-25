using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class RubixCube : AccessoryItem {
        public override string Title => "Puzzle Cube";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "Hmmmm... how peculiar.";

        public override int Price => 1000;

        public override RarityLevel Rarity => RarityLevel.Green;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(RubixCube)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(RubixCube)).GameObject;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Puzzle",
        };
    }
}