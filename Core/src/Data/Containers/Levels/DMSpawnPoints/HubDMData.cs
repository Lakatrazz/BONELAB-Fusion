using UnityEngine;

namespace LabFusion.Data
{
    public class HubDMData : DMLevelDataHandler
    {
        public override string LevelTitle => "02 - BONELAB Hub";

        protected override Vector3[] DeathmatchSpawnPoints => new Vector3[15] {
    new Vector3(-29.4879f, -2.9626f, 4.1192f),
    new Vector3(-8.5206f, 0.1815f, 0.0839f),
    new Vector3(8.0277f, -2.9218f, 4.6869f),
    new Vector3(-7.5653f, -3.9626f, 11.4138f),
    new Vector3(1.9645f, -4.9626f, 36.0565f),
    new Vector3(-8.5816f, -2.9625f, 48.0147f),
    new Vector3(7.4237f, -2.9218f, 48.0948f),
    new Vector3(7.5378f, -2.8572f, 19.2229f),
    new Vector3(-23.3114f, -2.9226f, 17.5164f),
    new Vector3(-25.5197f, -2.9247f, 47.7468f),
    new Vector3(-45.549f, 0.0375f, 33.964f),
    new Vector3(21.9294f, 0.0375f, 34.0221f),
    new Vector3(-4.3199f, 5.0374f, 51.6974f),
    new Vector3(-8.4574f, 25.0374f, 51.2673f),
    new Vector3(-4.0549f, 0.0375f, 39.6959f),
        };
    }
}
