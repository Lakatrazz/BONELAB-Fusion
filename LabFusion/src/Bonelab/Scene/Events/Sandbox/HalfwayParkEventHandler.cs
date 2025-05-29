using UnityEngine;

using LabFusion.SDK.Points;
using LabFusion.SDK.Scene;

namespace LabFusion.Bonelab.Scene;

public class HalfwayParkEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "fa534c5a83ee4ec6bd641fec424c4142.Level.LevelHalfwayPark";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(-85.0378f, 15.1181f, 85.4412f),
        new(9.0977f, 9.117f, 1.0171f),
        new(-49.9463f, 11.1174f, 85.6447f),
        new(-36.0197f, -5.8826f, 54.2049f),
        new(-79.8466f, -1.8826f, -9.0016f),
    };

    // Point shop setup
    public static readonly Vector3 PointShopPosition = new(23.5f, 0f, 56f);
    public static readonly Quaternion PointShopRotation = Quaternion.Euler(0f, -90f, 0f);

    protected override void OnLevelLoaded()
    {
        base.OnLevelLoaded();

        // Point shop
        PointShopHelper.SpawnBitMart(PointShopPosition, PointShopRotation);
    }
}
