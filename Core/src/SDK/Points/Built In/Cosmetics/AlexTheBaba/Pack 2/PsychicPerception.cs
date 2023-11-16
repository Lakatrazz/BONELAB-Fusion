using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class PsychicPerception : AccessoryItem
    {
        public override string Title => "Psychic Perception";

        public override string Author => BitEconomy.BaBaAuthor;

        public override string Description => "Explore your mind like some kind of... psychic astronaut...";

        public override int Price => 4500;

        public override RarityLevel Rarity => RarityLevel.Pink;

        public override Texture2D PreviewImage => FusionPointItemLoader.GetPair(nameof(PsychicPerception)).Preview;

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(PsychicPerception)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Effect",
            "Cosmetic",
            "Trance",
        };
    }
}