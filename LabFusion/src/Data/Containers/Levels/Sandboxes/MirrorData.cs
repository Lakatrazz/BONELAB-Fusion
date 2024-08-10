using UnityEngine;

using LabFusion.Extensions;
using LabFusion.SDK.Points;

namespace LabFusion.Data
{
    public class MirrorData : LevelDataHandler
    {
        public override string LevelTitle => "Mirror";

        // Point shop setup
        public static readonly Vector3 PointShopPosition = new(-4.5f, 0f, 1.65f);
        public static readonly Quaternion PointShopRotation = Quaternion.Euler(0f, 90f, 0f);

        protected override void MainSceneInitialized()
        {
            // Point shop
            PointShopHelper.SpawnBitMart(PointShopPosition, PointShopRotation);
        }
    }
}
