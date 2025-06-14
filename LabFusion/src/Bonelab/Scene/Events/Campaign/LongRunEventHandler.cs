using LabFusion.SDK.Scene;

using UnityEngine;

namespace LabFusion.Bonelab.Scene;

public class LongRunEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "c2534c5a-56a6-40ab-a8ce-23074c657665";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(36.1693f, -21.9632f, -148.1691f),
        new(36.2307f, -22.4638f, -138.8987f),
        new(27.9539f, -32.9626f, -146.4428f),
        new(35.8871f, -32.9626f, -130.9124f),
        new(28.6148f, -23.9616f, -163.052f),
        new(32.2719f, -29.9626f, -104.5819f),
        new(32.0926f, -32.8966f, -143.0124f),
    };
}
