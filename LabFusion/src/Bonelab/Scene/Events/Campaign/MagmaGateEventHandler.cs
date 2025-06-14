using UnityEngine;

using Il2CppSLZ.Bonelab;

using LabFusion.SDK.Scene;

namespace LabFusion.Bonelab.Scene;

public class MagmaGateEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "SLZ.BONELAB.Content.Level.SceneMagmaGate";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(-37.7922f, -0.1129f, 34.9711f),
        new(-14.8091f, 0.9308f, 35.5413f),
        new(4.7724f, 3.0374f, -0.7067f),
        new(7.6075f, 10.0373f, 1.4012f),
        new(-2.6123f, 12.0374f, 55.714f),
        new(-15.5539f, 6.0374f, 53.5095f),
        new(0.1097f, 12.0053f, 27.9264f),
        new(-23.6288f, 3.0374f, 0.9346f),
        new(-19.8211f, 15.0374f, 1.8029f),
        new(-14.2566f, 13.0374f, 52.8782f),
        new(16.1856f, 13.0374f, 53.5135f),
        new(14.9417f, 32.0374f, 57.1784f),
        new(6.3328f, 30.0374f, -0.4239f),
        new(-7.9078f, 12.0374f, -2.0455f),
        new(9.312f, 4.9901f, 29.5975f),
    };

    public static GameControl_MagmaGate GameController { get; set; }

    protected override void OnLevelLoaded()
    {
        base.OnLevelLoaded();

        GameController = GameObject.FindObjectOfType<GameControl_MagmaGate>(true);
    }
}
