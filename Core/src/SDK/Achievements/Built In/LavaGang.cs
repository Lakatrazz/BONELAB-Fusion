using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Achievements
{
    public class LavaGang : Achievement
    {
        public override string Title => "Lava Gang";

        public override string Description => "Spawn 1000 things across servers.";

        public override int BitReward => 1000;

        public override Texture2D PreviewImage => FusionAchievementLoader.GetPair(nameof(LavaGang)).Preview;

        public override int MaxTasks => 1000;
    }
}
