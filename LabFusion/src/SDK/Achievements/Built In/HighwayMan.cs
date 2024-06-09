using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.SDK.Achievements
{
    public class HighwayMan : Achievement
    {
        public override string Title => "Highway Man";

        public override string Description => "Grab something out of someone's holster.";

        public override int BitReward => 150;

        public override Texture2D PreviewImage => FusionAchievementLoader.GetPair(nameof(HighwayMan)).Preview;
    }
}
