using LabFusion.SDK.Scene;
using LabFusion.SDK.Points;

using Il2CppSLZ.Bonelab;

using UnityEngine;

namespace LabFusion.Bonelab.Scene;

public class BonelabHubEventHandler : GamemodeLevelEventHandler
{
    public override string LevelBarcode => "c2534c5a-6b79-40ec-8e98-e58c5363656e";

    public override Vector3[] GamemodeMarkerPoints => new Vector3[] {
        new(-29.4879f, -2.9626f, 4.1192f),
        new(-8.5206f, 0.1815f, 0.0839f),
        new(8.0277f, -2.9218f, 4.6869f),
        new(-7.5653f, -3.9626f, 11.4138f),
        new(1.9645f, -4.9626f, 36.0565f),
        new(-8.5816f, -2.9625f, 48.0147f),
        new(7.4237f, -2.9218f, 48.0948f),
        new(7.5378f, -2.8572f, 19.2229f),
        new(-23.3114f, -2.9226f, 17.5164f),
        new(-25.5197f, -2.9247f, 47.7468f),
        new(-45.549f, 0.0375f, 33.964f),
        new(21.9294f, 0.0375f, 34.0221f),
        new(-4.3199f, 5.0374f, 51.6974f),
        new(-8.4574f, 25.0374f, 51.2673f),
        new(-4.0549f, 0.0375f, 39.6959f),
    };

    public static GameControl_Hub GameController { get; set; }

    public static readonly Vector3 PointShopPosition = new Vector3(-5.69f, -0.013f, 39.79f);
    public static readonly Quaternion PointShopRotation = Quaternion.Euler(0f, 90f, 0f);

    protected override void OnLevelLoaded()
    {
        base.OnLevelLoaded();

        GameController = GameObject.FindObjectOfType<GameControl_Hub>(true);

        if (GameController == null)
        {
            return;
        }

        PointShopHelper.SpawnBitMart(PointShopPosition, PointShopRotation);
    }
}