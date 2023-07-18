using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points {
    public class SignalingSpiral : AccessoryItem {
        public override string Title => "Signaling Spiral";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "The Spiraling Shape will make you go insane!";

        public override int Price => 6000;

        public override RarityLevel Rarity => RarityLevel.Red;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(SignalingSpiral)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(SignalingSpiral)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Effect",
            "Cosmetic",
            "Wavy",
        };
    }
}