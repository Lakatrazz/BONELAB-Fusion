using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class Briefcase : AccessoryItem
    {
        public override string Title => "Briefcase";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "Fliggol Blueprints Inside.";

        public override int Price => 900;

        public override RarityLevel Rarity => RarityLevel.Blue;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(Briefcase)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(Briefcase)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.CHEST_BACK;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEIGHT;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Backpack",
            "Cosmetic",
            "Work",
        };
    }
}