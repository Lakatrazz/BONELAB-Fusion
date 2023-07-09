using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Achievements
{
    public class WarmWelcome : Achievement
    {
        public override string Title => "Warm Welcome";

        public override string Description => "Join a server.";

        public override int BitReward => 50;

        public override Texture2D PreviewImage => FusionAchievementLoader.GetPair(nameof(WarmWelcome)).Preview;
    }
}
