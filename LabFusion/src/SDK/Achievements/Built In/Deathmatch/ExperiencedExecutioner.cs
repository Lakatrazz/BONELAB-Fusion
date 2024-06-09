using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.SDK.Achievements
{
    public class ExperiencedExecutioner : KillerAchievement
    {
        public override string Title => "Experienced Executioner";

        public override string Description => "Kill 100 players in Deathmatch or Team Deathmatch.";

        public override int BitReward => 200;

        public override Texture2D PreviewImage => FusionAchievementLoader.GetPair(nameof(ExperiencedExecutioner)).Preview;

        public override int MaxTasks => 100;
    }
}
