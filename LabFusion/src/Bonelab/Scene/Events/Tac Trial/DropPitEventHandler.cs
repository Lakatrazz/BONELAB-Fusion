using LabFusion.SDK.Scene;

using UnityEngine;

namespace LabFusion.Bonelab.Scene;

public class DropPitEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "c2534c5a-de61-4df9-8f6c-416954726547";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(-19.6002f, -2.4644f, -7.0592f),
        new(-14.2666f, -0.4644f, 2.0659f),
        new(-14.7582f, -0.4626f, -21.4207f),
        new(-15.1494f, -0.4644f, -10.5613f),
        new(-10.0619f, -0.4644f, -10.7547f),
        new(-8.8003f, -2.4644f, -11.9671f),
        new(-19.1125f, -0.4626f, -21.8052f),
        new(-6.1433f, -0.4626f, -22.927f),
    };
}
