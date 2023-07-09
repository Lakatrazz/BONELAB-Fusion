using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Achievements
{
    public class HeadOfHouse : Achievement
    {
        public override string Title => "Head Of House";

        public override string Description => "Start a server.";

        public override int BitReward => 50;

        public override Texture2D PreviewImage => FusionAchievementLoader.GetPair(nameof(HeadOfHouse)).Preview;
    }
}
