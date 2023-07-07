using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Achievements
{
    public class RookieAssassin : Achievement
    {
        public override string Title => "Rookie Assassin";

        public override string Description => "Kill your first player in Deathmatch or Team Deathmatch.";

        public override int BitReward => 10;

        public override Texture2D PreviewImage => FusionAchievementLoader.GetPair(nameof(RookieAssassin)).Preview;
    }
}
