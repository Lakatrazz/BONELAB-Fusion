using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class VirtuallyInsane : AccessoryItem {
        public override string Title => "Virtually Insane";

        public override string Author => BitEconomy.RiggleAuthor;

        public override string Description => "What we're livin' in, lemme tell y'all";

        public override int Price => 800;

        public override RarityLevel Rarity => RarityLevel.Orange;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(VirtuallyInsane)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(VirtuallyInsane)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Crazy",
        };
    }
}