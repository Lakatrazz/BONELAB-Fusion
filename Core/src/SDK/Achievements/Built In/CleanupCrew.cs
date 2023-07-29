using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Achievements
{
    public class CleanupCrew : Achievement
    {
        public override string Title => "Cleanup Crew";

        public override string Description => "Despawn 1000 things across servers.";

        public override int BitReward => 1000;

        public override Texture2D PreviewImage => FusionAchievementLoader.GetPair(nameof(CleanupCrew)).Preview;

        public override int MaxTasks => 1000;
    }
}
