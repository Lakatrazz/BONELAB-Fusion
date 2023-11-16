using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class CBRNGasMask : AccessoryItem
    {
        public override string Title => "CBRN Gas Mask";

        public override string Author => BitEconomy.RiggleAuthor;

        public override string Description => "For the BONELAB fog machine";

        public override int Price => 2300;

        public override RarityLevel Rarity => RarityLevel.Yellow;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(CBRNGasMask)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(CBRNGasMask)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.NOSE;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Gas",
        };
    }
}