using UnityEngine;

using LabFusion.SDK.Points;
using LabFusion.SDK.Scene;

namespace LabFusion.Bonelab.Scene;

public class TuscanyEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "c2534c5a-2c4c-4b44-b076-203b5363656e";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(17.886f, 6.9674f, 54.4874f),
        new(-39.3922f, 0.7033f, 7.8235f),
        new(-1.6841f, 12.3276f, 5.5593f),
        new(27.6902f, -0.3047f, -1.3649f),
        new(-2.8871f, 1.4279f, 21.9112f),
        new(0.0901f, 5.7132f, -0.9069f),
        new(-6.3986f, 4.3902f, 6.615f),
        new(-18.9443f, 1.4807f, -6.1819f),
        new(0.7016f, 1.4772f, -16.6518f),
        new(14.0029f, 1.8974f, 4.1357f),
    };

    // Point shop setup
    public static readonly Vector3 PointShopPosition = new(-9f, 0f, 8f);
    public static readonly Quaternion PointShopRotation = Quaternion.Euler(0f, -90f, 0f);

    protected override void OnLevelLoaded()
    {
        base.OnLevelLoaded();

        // Point shop
        PointShopHelper.SpawnBitMart(PointShopPosition, PointShopRotation);
    }
}
