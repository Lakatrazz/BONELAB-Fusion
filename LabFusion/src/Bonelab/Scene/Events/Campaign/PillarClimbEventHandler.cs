using LabFusion.SDK.Scene;

using UnityEngine;

namespace LabFusion.Bonelab.Scene;

public class PillarClimbEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "c2534c5a-c056-4883-ac79-e051426f6964";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(3.7457f, 0.0374f, -1.5809f),
        new(0.8794f, 8.0174f, 1.4217f),
        new(0.3373f, 14.8141f, 0.2999f),
        new(3.9943f, 24.1792f, -0.2227f),
        new(15.8407f, 48.7708f, 6.105f),
        new(8.5758f, 35.8305f, -0.3652f),
        new(10.5365f, 49.3929f, -4.7714f),
        new(-4.1714f, 36.5375f, -0.3992f),
    };
}
