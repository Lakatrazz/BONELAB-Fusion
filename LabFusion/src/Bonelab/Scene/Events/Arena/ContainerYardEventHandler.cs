using LabFusion.SDK.Scene;

using UnityEngine;

namespace LabFusion.Bonelab.Scene;

public class ContainerYardEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "c2534c5a-162f-4661-a04d-975d5363656e";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(-0.1157f, 0.0374f, -21.7821f),
        new(-5.7191f, 8.3797f, 0.8318f),
        new(-17.6157f, 4.8145f, 4.1608f),
        new(-18.5699f, 0.0374f, -7.3159f),
        new(-28.1128f, 0.2006f, -3.6932f),
        new(-20.6262f, 16.4323f, -6.8991f),
        new(-0.559f, 11.932f, 27.5109f),
        new(-15.5463f, 1.3338f, 26.1137f),
        new(-20.334f, 7.4489f, -28.236f),
        new(-27.9585f, 4.3168f, -12.8561f),
        new(22.0699f, 1.3118f, 22.1839f),
        new(-0.1958f, 1.3564f, 13.5414f),
        new(22.9186f, 4.7433f, -0.8553f),
        new(16.6211f, 1.2044f, -27.1501f),
    };
}
