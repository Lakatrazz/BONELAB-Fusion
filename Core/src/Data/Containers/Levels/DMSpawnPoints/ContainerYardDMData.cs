using UnityEngine;

namespace LabFusion.Data
{
    public class ContainerYardDMData : DMLevelDataHandler
    {
        public override string LevelTitle => "Container Yard";

        protected override Vector3[] DeathmatchSpawnPoints => new Vector3[14] {
    new Vector3(-0.1157f, 0.0374f, -21.7821f),
    new Vector3(-5.7191f, 8.3797f, 0.8318f),
    new Vector3(-17.6157f, 4.8145f, 4.1608f),
    new Vector3(-18.5699f, 0.0374f, -7.3159f),
    new Vector3(-28.1128f, 0.2006f, -3.6932f),
    new Vector3(-20.6262f, 16.4323f, -6.8991f),
    new Vector3(-0.559f, 11.932f, 27.5109f),
    new Vector3(-15.5463f, 1.3338f, 26.1137f),
    new Vector3(-20.334f, 7.4489f, -28.236f),
    new Vector3(-27.9585f, 4.3168f, -12.8561f),
    new Vector3(22.0699f, 1.3118f, 22.1839f),
    new Vector3(-0.1958f, 1.3564f, 13.5414f),
    new Vector3(22.9186f, 4.7433f, -0.8553f),
    new Vector3(16.6211f, 1.2044f, -27.1501f),
        };
    }
}
