using LabFusion.SDK.Scene;

using UnityEngine;

namespace LabFusion.Bonelab.Scene;

public class SprintBridgeEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "SLZ.BONELAB.Content.Level.SprintBridge04";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(2.3381f, 31.5374f, 134.9037f),
        new(-0.4947f, 32.0374f, 94.5794f),
        new(14.106f, 32.0373f, 79.3274f),
        new(56.818f, 32.0373f, 81.8022f),
        new(-4.9095f, 33.971f, 68.9549f),
        new(0.169f, 31.5374f, 25.0182f),
        new(-1.2044f, 24.0373f, -13.917f),
        new(-0.2945f, 18.5374f, 179.6593f),
    };
}
