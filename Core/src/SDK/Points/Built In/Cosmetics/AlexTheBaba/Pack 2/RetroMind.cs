using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class RetroMind : AccessoryItem
    {
        public override string Title => "Retro Mind";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "MY LIFE IS LIKE A VIDEO GAME";

        public override int Price => 6000;

        public override RarityLevel Rarity => RarityLevel.Pink;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(RetroMind)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(RetroMind)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Effect",
            "Cosmetic",
            "Retro",
        };
    }
}