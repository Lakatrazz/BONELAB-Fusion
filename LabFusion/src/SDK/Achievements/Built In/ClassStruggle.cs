using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.SDK.Achievements
{
    public class ClassStruggle : Achievement
    {
        public override string Title => "Class Struggle";

        public override string Description => "Get constrained by the server host.";

        public override int BitReward => 700;

        public override Texture2D PreviewImage => FusionAchievementLoader.GetPair(nameof(ClassStruggle)).Preview;
    }
}
