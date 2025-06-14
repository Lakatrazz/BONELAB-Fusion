using LabFusion.SDK.Scene;

using UnityEngine;

namespace LabFusion.Bonelab.Scene;

public class NeonDistrictParkourEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "fa534c5a83ee4ec6bd641fec424c4142.Level.SceneparkourDistrictLogic";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(14.1986f, 0.9377f, -7.2248f),
        new(4.0275f, 4.3721f, -3.6219f),
        new(17.0486f, 3.0206f, -14.0138f),
        new(13.5923f, 1.1912f, -13.8777f),
        new(18.5329f, 4.9916f, -24.4213f),
        new(8.154f, 3.3383f, -30.4304f),
        new(-3.1803f, 8.2037f, -31.3292f),
        new(-13.4639f, 3.3676f, -29.011f),
        new(-9.9654f, 1.3233f, -21.4282f),
        new(-9.157f, 1.3293f, -9.5022f),
        new(-2.9644f, 1.2189f, -16.9085f),
        new(1.1659f, 1.2677f, -7.5986f),
    };
}
