using LabFusion.SDK.Scene;

using UnityEngine;

namespace LabFusion.Bonelab.Scene;

public class BigAnomalyEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "c2534c5a-7601-4443-bdfe-7f235363656e";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[9] {
        new(4.4089f, 40.0375f, 108.1976f),
        new(27.662f, 43.0375f, 93.3687f),
        new(-9.566f, 40.0375f, 103.2718f),
        new(27.6981f, 20.0375f, 94.1142f),
        new(32.4794f, 25.0374f, 97.0537f),
        new(37.5303f, 25.0374f, 84.4273f),
        new(26.2376f, 28.0375f, 103.2391f),
        new(41.3916f, 25.0374f, 60.6696f),
        new(29.4577f, 25.0374f, 61.6327f),
    };
}
