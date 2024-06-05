using UnityEngine;

namespace LabFusion.Data
{
    public class LongRunDMData : DMLevelDataHandler
    {
        public override string LevelTitle => "03 - LongRun";

        protected override Vector3[] DeathmatchSpawnPoints => new Vector3[7] {
    new Vector3(36.1693f, -21.9632f, -148.1691f),
    new Vector3(36.2307f, -22.4638f, -138.8987f),
    new Vector3(27.9539f, -32.9626f, -146.4428f),
    new Vector3(35.8871f, -32.9626f, -130.9124f),
    new Vector3(28.6148f, -23.9616f, -163.052f),
    new Vector3(32.2719f, -29.9626f, -104.5819f),
    new Vector3(32.0926f, -32.8966f, -143.0124f),
        };
    }
}
