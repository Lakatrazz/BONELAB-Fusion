using LabFusion.SDK.Scene;

using UnityEngine;

namespace LabFusion.Bonelab.Scene;

public class MainMenuEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "c2534c5a-80e1-4a29-93ca-f3254d656e75";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(29.8048f, -1.1377f, 0.8369f),
        new(26.6286f, -1.1377f, 0.9376f),
        new(29.5723f, -1.1377f, -1.0273f),
        new(26.4974f, -1.1377f, -1.1975f),
    };
}
