using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Syncables;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
