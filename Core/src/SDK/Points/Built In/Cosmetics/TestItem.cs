using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class TestItem : AccessoryItem {
        public override string Title => "Test Item Please Ignore";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "Test item description! Replace this!";

        public override string Version => "0.0.0";

        public override int Price => 3200;

        public override RarityLevel Rarity => RarityLevel.Cyan;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(TestItem)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(TestItem)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Test",
        };
    }
}