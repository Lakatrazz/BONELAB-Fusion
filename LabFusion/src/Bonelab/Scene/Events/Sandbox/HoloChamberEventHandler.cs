using Il2CppSLZ.Bonelab;

using UnityEngine;

using LabFusion.SDK.Scene;

namespace LabFusion.Bonelab.Scene;

public class HoloChamberEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "fa534c5a83ee4ec6bd641fec424c4142.Level.LevelHoloChamber";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(18.5182f, 0.0371f, 3.7177f),
        new(18.5824f, 0.0371f, 40.6872f),
        new(0.0182f, 0.0374f, 0.8058f),
        new(-18.5055f, 0.0371f, 40.3152f),
        new(-18.5734f, 0.0371f, 3.203f),
        new(-0.14f, 0.0371f, 21.9039f),
    };

    public static GameControl_Holodeck GameController { get; set; }
    protected override void OnLevelLoaded()
    {
        base.OnLevelLoaded();

        GameController = GameObject.FindObjectOfType<GameControl_Holodeck>(true);
    }
}
