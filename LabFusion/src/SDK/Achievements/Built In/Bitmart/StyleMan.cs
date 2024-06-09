using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.SDK.Achievements
{
    public class StyleMan : StyleAchievement
    {
        public override string Title => "Style Man";

        public override string Description => "Purchase your tenth cosmetic.";

        public override int BitReward => 1000;

        public override Texture2D PreviewImage => FusionAchievementLoader.GetPair(nameof(StyleMan)).Preview;

        public override int MaxTasks => 10;
    }
}
