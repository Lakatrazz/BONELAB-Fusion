using UnityEngine;

using LabFusion.Extensions;
using LabFusion.SDK.Points;

namespace LabFusion.Data
{
    public class BigBoneBowlingData : LevelDataHandler
    {
        public override string LevelTitle => "Big Bone Bowling";

        // Point shop setup
        public static readonly Vector3 PointShopPosition = new(28.5f, 1f, -50.5f);
        public static readonly Quaternion PointShopRotation = Quaternion.Euler(0f, -90f, 0f);

        protected override void MainSceneInitialized()
        {
            // Point shop
            PointShopHelper.SpawnBitMart(PointShopPosition, PointShopRotation);
        }
    }
}
