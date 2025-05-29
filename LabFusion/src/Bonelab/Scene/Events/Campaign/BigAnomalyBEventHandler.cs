using UnityEngine;

using LabFusion.SDK.Scene;

namespace LabFusion.Bonelab.Scene;

public class BigAnomalyBEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "SLZ.BONELAB.Content.Level.LevelBigAnomalyB";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(-29.0873f, 30.0374f, 53.9518f),
        new(-27.3884f, 25.0374f, 84.7466f),
        new(-18.74f, 25.0374f, 108.7661f),
        new(-15.6366f, 25.0373f, 124.4391f),
        new(-25.2926f, 25.0373f, 124.2993f),
        new(-27.2077f, 30.0374f, 83.2289f),
    };
}
