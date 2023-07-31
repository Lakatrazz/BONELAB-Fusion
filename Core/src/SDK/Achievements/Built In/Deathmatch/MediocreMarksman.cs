using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.SDK.Achievements
{
    public class MediocreMarksman : KillerAchievement
    {
        public override string Title => "Mediocre Marksman";

        public override string Description => "Kill 10 players in Deathmatch or Team Deathmatch.";

        public override int BitReward => 50;

        public override Texture2D PreviewImage => FusionAchievementLoader.GetPair(nameof(MediocreMarksman)).Preview;

        public override int MaxTasks => 10;
    }
}
