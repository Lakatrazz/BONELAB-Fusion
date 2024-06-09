using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.SDK.Achievements
{
    public class StyleBaby : StyleAchievement
    {
        public override string Title => "Style Baby";

        public override string Description => "Purchase your first cosmetic.";

        public override int BitReward => 50;

        public override Texture2D PreviewImage => FusionAchievementLoader.GetPair(nameof(StyleBaby)).Preview;

        public override int MaxTasks => 1;
    }
}
