using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class FlinchsJinjle : AccessoryItem
    {
        public override string Title => "Flinchs Jinjle";

        public override string Author => BitEconomy.RiggleAuthor;

        public override string Description => "Did you survive?";

        public override int Price => 4500;

        public override RarityLevel Rarity => RarityLevel.Purple;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(FlinchsJinjle)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(FlinchsJinjle)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Bloody",
        };
    }
}