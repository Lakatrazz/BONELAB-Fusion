using UnityEngine;

namespace LabFusion.Data
{
    public class VoidG114DMData : DMLevelDataHandler
    {
        public override string LevelTitle => "15 - Void G114";

        protected override Vector3[] DeathmatchSpawnPoints => new Vector3[8] {
    new Vector3(28.0497f, -0.2327f, 0.0443f),
    new Vector3(27.741f, 4.8885f, -5.717f),
    new Vector3(27.5797f, 4.6144f, -6.9054f),
    new Vector3(28.0605f, -0.1453f, 4.4828f),
    new Vector3(39.0173f, -0.7884f, 15.841f),
    new Vector3(13.4692f, -0.8171f, 9.5978f),
    new Vector3(18.9537f, -0.8847f, -10.8288f),
    new Vector3(35.9514f, -0.8819f, -12.1057f),
        };
    }
}
