using UnityEngine;

using LabFusion.SDK.Points;
using LabFusion.SDK.Scene;

namespace LabFusion.Bonelab.Scene;

public class BigBoneBowlingEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "fa534c5a83ee4ec6bd641fec424c4142.Level.LevelKartBowling";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(16.2324f, -25.712f, 61.4863f),
        new(-21.725f, -25.712f, 24.8524f),
        new(-15.5071f, -25.7122f, 54.1341f),
        new(-21.2391f, -25.7125f, 99.1708f),
        new(15.4044f, -25.7121f, 81.8368f),
        new(48.8102f, -25.7124f, 81.8609f),
        new(53.8504f, -25.7125f, 99.2813f),
        new(53.2528f, -25.712f, 61.801f),
        new(48.7699f, -25.7121f, 29.7422f),
        new(24.4213f, -25.7121f, 30.5197f),
        new(-4.5168f, -25.7123f, 70.1425f),
    };

    // Point shop setup
    public static readonly Vector3 PointShopPosition = new(28.5f, 1f, -50.5f);
    public static readonly Quaternion PointShopRotation = Quaternion.Euler(0f, -90f, 0f);

    protected override void OnLevelLoaded()
    {
        base.OnLevelLoaded();

        // Point shop
        PointShopHelper.SpawnBitMart(PointShopPosition, PointShopRotation);
    }
}
