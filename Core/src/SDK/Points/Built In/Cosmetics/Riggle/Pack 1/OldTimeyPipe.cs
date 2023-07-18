using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class OldTimeyPipe : AccessoryItem {
        public override string Title => "Old Timey Pipe";

        public override string Author => BitEconomy.RiggleAuthor;

        public override string Description => "Who knows what's in there... (note: I do)";

        public override int Price => 2500;

        public override RarityLevel Rarity => RarityLevel.Pink;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(OldTimeyPipe)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(OldTimeyPipe)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.EYE_CENTER;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Dirty",
        };
    }
}