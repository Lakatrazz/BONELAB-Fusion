using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.SDK.Achievements
{
    public class HelloThere : Achievement
    {
        public override string Title => "Hello There";

        public override string Description => "Discover the secret in the Information Box.";

        public override int BitReward => 10;

        public override Texture2D PreviewImage => FusionAchievementLoader.GetPair(nameof(HelloThere)).Preview;
    }
}
