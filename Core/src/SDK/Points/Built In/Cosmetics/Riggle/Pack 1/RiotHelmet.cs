using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class RiotHelmet : AccessoryItem
    {
        public override string Title => "Riot Helmet";

        public override string Author => BitEconomy.RiggleAuthor;

        public override string Description => "Valued armor of a ZaZa warrior.";

        public override int Price => 1120;

        public override RarityLevel Rarity => RarityLevel.Orange;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(RiotHelmet)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(RiotHelmet)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Warrior",
        };
    }
}