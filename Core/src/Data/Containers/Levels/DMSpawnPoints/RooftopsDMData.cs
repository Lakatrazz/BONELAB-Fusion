using UnityEngine;

namespace LabFusion.Data
{
    public class RooftopsDMData : DMLevelDataHandler
    {
        public override string LevelTitle => "Rooftops";

        protected override Vector3[] DeathmatchSpawnPoints => new Vector3[14] {
    new Vector3(59.4848f, 93.0314f, -54.4698f),
    new Vector3(74.7662f, 90.0374f, -57.4251f),
    new Vector3(60.3601f, 92.2162f, -69.01f),
    new Vector3(53.8805f, 90.197f, -66.1647f),
    new Vector3(42.8871f, 90.197f, -66.5409f),
    new Vector3(35.5215f, 90.7248f, -60.718f),
    new Vector3(51.6421f, 93.698f, -55.6575f),
    new Vector3(45.2663f, 79.6913f, -38.3841f),
    new Vector3(59.7499f, 90.0374f, -53.5572f),
    new Vector3(42.109f, 93.569f, -66.7509f),
    new Vector3(37.153f, 90.0374f, -54.4797f),
    new Vector3(37.6532f, 90.197f, -66.376f),
    new Vector3(64.6106f, 93.5873f, -62.8519f),
    new Vector3(69.8322f, 99.0641f, -68.1835f),
        };
    }
}
