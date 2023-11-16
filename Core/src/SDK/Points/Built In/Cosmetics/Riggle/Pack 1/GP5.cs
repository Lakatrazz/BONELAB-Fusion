using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class GP5 : AccessoryItem
    {
        public override string Title => "GP-5 Gas Mask";

        public override string Author => BitEconomy.RiggleAuthor;

        public override string Description => "The air is tough out there";

        public override int Price => 1400;

        public override RarityLevel Rarity => RarityLevel.LightPurple;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(GP5)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(GP5)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Gas",
        };
    }
}