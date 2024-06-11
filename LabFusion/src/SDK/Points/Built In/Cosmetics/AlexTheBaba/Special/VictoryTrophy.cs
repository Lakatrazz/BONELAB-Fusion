using LabFusion.SDK.Achievements;

using LabFusion.Utilities;
using UnityEngine;

namespace LabFusion.SDK.Points
{
    public class VictoryTrophy : AccessoryItem
    {
        public override string Title => "Victory Trophy";

        public override string Author => "BaBaCorp";

        public override string Description => "YOU WON! YOU DID IT! YOU WON <b>THE GAME!</b>";

        public override int Price => BitEconomy.PricelessValue;

        public override bool Redacted => true;

        public override bool Priceless => true;

        public override bool IsUnlocked => AchievementManager.IsCompleted();

        public override GameObject AccessoryPrefab => FusionPointItemLoader.GetPair(nameof(VictoryTrophy)).GameObject;

        public override AccessoryPoint ItemPoint => AccessoryPoint.HEAD_TOP;

        public override AccessoryScaleMode ScaleMode => AccessoryScaleMode.HEAD;

        public override bool IsHiddenInView => true;

        public override string[] Tags => new string[3] {
            "Hat",
            "Cosmetic",
            "Completionist",
        };

        public override void LoadPreviewIcon(Action<Texture2D> onLoaded)
        {
            onLoaded(FusionPointItemLoader.GetPair(nameof(VictoryTrophy)).Preview);
        }
    }
}