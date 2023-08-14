using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public class PillarClimbDMData : DMLevelDataHandler {
        public override string LevelTitle => "11 - Pillar Climb";

        protected override Vector3[] DeathmatchSpawnPoints => new Vector3[8] {
    new Vector3(3.7457f, 0.0374f, -1.5809f),
    new Vector3(0.8794f, 8.0174f, 1.4217f),
    new Vector3(0.3373f, 14.8141f, 0.2999f),
    new Vector3(3.9943f, 24.1792f, -0.2227f),
    new Vector3(15.8407f, 48.7708f, 6.105f),
    new Vector3(8.5758f, 35.8305f, -0.3652f),
    new Vector3(10.5365f, 49.3929f, -4.7714f),
    new Vector3(-4.1714f, 36.5375f, -0.3992f),
        };
    }
}
