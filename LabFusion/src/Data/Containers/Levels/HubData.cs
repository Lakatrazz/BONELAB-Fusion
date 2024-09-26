using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.SDK.Points;

using Il2CppSLZ.Bonelab;

using UnityEngine;

namespace LabFusion.Data;

public class HubData : LevelDataHandler
{
    public override string LevelTitle => "02 - BONELAB Hub";

    public static GameControl_Hub GameController;

    public static readonly Vector3 PointShopPosition = new Vector3(-5.69f, -0.013f, 39.79f);
    public static readonly Quaternion PointShopRotation = Quaternion.Euler(0f, 90f, 0f);

    protected override void MainSceneInitialized()
    {
        GameController = GameObject.FindObjectOfType<GameControl_Hub>(true);

        if (GameController == null)
        {
            return;
        }

        PointShopHelper.SpawnBitMart(PointShopPosition, PointShopRotation);
    }
}