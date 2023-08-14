using LabFusion.MarrowIntegration;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class BigBoneBowlingDMData : DMLevelDataHandler {
        public override string LevelTitle => "Big Bone Bowling";

        protected override Vector3[] DeathmatchSpawnPoints => new Vector3[11] {
    new Vector3(16.2324f, -25.712f, 61.4863f),
    new Vector3(-21.725f, -25.712f, 24.8524f),
    new Vector3(-15.5071f, -25.7122f, 54.1341f),
    new Vector3(-21.2391f, -25.7125f, 99.1708f),
    new Vector3(15.4044f, -25.7121f, 81.8368f),
    new Vector3(48.8102f, -25.7124f, 81.8609f),
    new Vector3(53.8504f, -25.7125f, 99.2813f),
    new Vector3(53.2528f, -25.712f, 61.801f),
    new Vector3(48.7699f, -25.7121f, 29.7422f),
    new Vector3(24.4213f, -25.7121f, 30.5197f),
    new Vector3(-4.5168f, -25.7123f, 70.1425f),
        };
    }
}
