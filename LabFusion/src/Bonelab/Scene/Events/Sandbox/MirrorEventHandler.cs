using UnityEngine;

using LabFusion.SDK.Points;
using LabFusion.SDK.Scene;

namespace LabFusion.Bonelab.Scene;

public class MirrorEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "SLZ.BONELAB.Content.Level.LevelMirror";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(-0.3258f, 1.3078f, 2.3584f),
        new(-12.8975f, 1.3411f, -12.7348f),
        new(-11.0187f, 1.2294f, 0.2149f),
        new(0.1774f, 1.2333f, -10.9054f),
        new(-13.9262f, 10.2407f, -14.7184f),
        new(-11.6317f, 10.1571f, -0.3718f),
        new(-1.2534f, 10.245f, -9.7245f),
        new(-1.3073f, 10.8671f, -0.2344f),
    };

    // Point shop setup
    public static readonly Vector3 PointShopPosition = new(-4.5f, 0f, 1.65f);
    public static readonly Quaternion PointShopRotation = Quaternion.Euler(0f, 90f, 0f);

    protected override void OnLevelLoaded()
    {
        base.OnLevelLoaded();

        // Point shop
        PointShopHelper.SpawnBitMart(PointShopPosition, PointShopRotation);
    }
}
