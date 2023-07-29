using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
