using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.SDK.Achievements
{
    public class Betrayal : Achievement
    {
        public override string Title => "Betrayal";

        public override string Description => "Save the hung peasant in Descent.";

        public override int BitReward => 300;

        public override Texture2D PreviewImage => FusionAchievementLoader.GetPair(nameof(Betrayal)).Preview;
    }
}
