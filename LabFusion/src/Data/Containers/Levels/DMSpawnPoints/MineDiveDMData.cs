using UnityEngine;

namespace LabFusion.Data
{
    public class MineDiveDMData : DMLevelDataHandler
    {
        public override string LevelTitle => "04 - Mine Dive";

        protected override Vector3[] DeathmatchSpawnPoints => new Vector3[6] {
    new Vector3(-5.6735f, -0.0314f, -11.8826f),
    new Vector3(-10.8307f, -0.0054f, 11.0599f),
    new Vector3(-11.1408f, 0.3384f, -23.1127f),
    new Vector3(-5.2951f, -0.1026f, -34.264f),
    new Vector3(-1.4603f, -0.0231f, -24.2549f),
    new Vector3(3.4105f, -0.0699f, 10.5926f),
        };
    }
}
