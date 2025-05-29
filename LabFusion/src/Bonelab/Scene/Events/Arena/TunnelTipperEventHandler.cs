using LabFusion.SDK.Scene;

using UnityEngine;

namespace LabFusion.Bonelab.Scene;

public class TunnelTipperEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "c2534c5a-c180-40e0-b2b7-325c5363656e";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(10.0078f, 16.0968f, 21.5753f),
        new(10.0832f, 15.663f, 25.6005f),
        new(9.9641f, 18.1093f, -20.7673f),
        new(10.595f, 15.1221f, -15.0623f),
        new(11.1202f, 1.1871f, 10.3299f),
        new(6.7449f, 6.9961f, 2.9742f),
        new(11.9569f, 7.818f, -4.9224f),
        new(9.2211f, -9.798f, 26.545f),
        new(9.386f, 9.9136f, -15.1336f),
        new(9.7402f, 10.0533f, 18.4418f),
    };
}
