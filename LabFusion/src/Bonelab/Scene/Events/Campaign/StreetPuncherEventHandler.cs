using LabFusion.SDK.Scene;

using UnityEngine;

namespace LabFusion.Bonelab.Scene;

public class StreetPuncherEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "SLZ.BONELAB.Content.Level.LevelStreetPunch";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(-1.4924f, 11.7874f, 94.0635f),
        new(3.5141f, 13.4904f, 101.4765f),
        new(9.0983f, 17.7881f, 134.1948f),
        new(15.2911f, 11.7874f, 103.3306f),
        new(0.5954f, 7.7874f, 90.02f),
        new(20.7147f, 1.95f, 93.0073f),
        new(9.5308f, 1.7873f, 82.1337f),
        new(3.2431f, 1.7874f, 135.6179f),
        new(-4.844f, 2.0374f, 111.886f),
        new(13.5487f, 9.7874f, 122.6553f),
        new(6.6608f, 11.3569f, 110.1438f),
        new(-5.1882f, 17.7874f, 134.6813f),
    };
}
