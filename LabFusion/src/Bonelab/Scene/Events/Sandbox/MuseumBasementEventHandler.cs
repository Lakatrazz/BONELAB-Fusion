using UnityEngine;

using LabFusion.Marrow.Scene;
using LabFusion.SDK.Points;

namespace LabFusion.Bonelab.Scene;

public class MuseumBasementEventHandler : LevelEventHandler
{
    public override string LevelBarcode => "fa534c5a83ee4ec6bd641fec424c4142.Level.LevelMuseumBasement";

    // Point shop setup
    public static readonly Vector3 PointShopPosition = new(31.5f, 0f, 27f);
    public static readonly Quaternion PointShopRotation = Quaternion.Euler(0f, 90f, 0f);

    protected override void OnLevelLoaded()
    {
        // Point shop
        PointShopHelper.SpawnBitMart(PointShopPosition, PointShopRotation);
    }
}
