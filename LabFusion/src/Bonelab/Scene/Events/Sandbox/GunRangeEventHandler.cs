using UnityEngine;

using LabFusion.SDK.Points;
using LabFusion.SDK.Scene;

namespace LabFusion.Bonelab.Scene;

public class GunRangeEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "fa534c5a83ee4ec6bd641fec424c4142.Level.LevelGunRange";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(-17.3122f, 0.0374f, -48.4204f),
        new(-1.2045f, 0.0374f, -30.4701f),
        new(-1.2281f, 0.0374f, -41.9648f),
        new(-1.4794f, 0.0461f, -18.6144f),
        new(-9.3656f, 0.0374f, -7.2933f),
        new(-16.5774f, 0.0374f, -11.0135f),
        new(-7.7083f, 0.0374f, -24.4492f),
        new(-13.5419f, 0.0374f, -35.0949f),
    };

    // Point shop setup
    public static readonly Vector3 PointShopPosition = new(2f, 0f, -18.5f);
    public static readonly Quaternion PointShopRotation = Quaternion.Euler(0f, -90f, 0f);

    protected override void OnLevelLoaded()
    {
        base.OnLevelLoaded();

        // Point shop
        PointShopHelper.SpawnBitMart(PointShopPosition, PointShopRotation);
    }
}
