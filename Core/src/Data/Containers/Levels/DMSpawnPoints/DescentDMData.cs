using UnityEngine;

namespace LabFusion.Data
{
    public class DescentDMData : DMLevelDataHandler
    {
        public override string LevelTitle => "01 - Descent";

        protected override Vector3[] DeathmatchSpawnPoints => new Vector3[12] {
    new Vector3(123.9262f, -62.2602f, 166.4684f),
    new Vector3(125.4921f, -72.8109f, 186.0882f),
    new Vector3(122.0129f, -65.7702f, 192.8663f),
    new Vector3(107.1931f, -62.2902f, 191.8245f),
    new Vector3(87.1881f, -27.7636f, 184.0406f),
    new Vector3(135.6338f, -72.2126f, 210.5421f),
    new Vector3(123.1387f, -65.8102f, 216.6858f),
    new Vector3(107.2842f, -62.2902f, 214.4001f),
    new Vector3(107.0591f, -65.8102f, 173.6062f),
    new Vector3(158.7357f, -68.2125f, 210.362f),
    new Vector3(115.0106f, -72.8502f, 162.9939f),
    new Vector3(123.6596f, -72.8502f, 229.1833f),
        };
    }
}
