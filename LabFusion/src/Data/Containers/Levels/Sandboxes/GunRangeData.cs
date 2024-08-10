using UnityEngine;

using LabFusion.Extensions;
using LabFusion.SDK.Points;

namespace LabFusion.Data
{
    public class GunRangeData : LevelDataHandler
    {
        public override string LevelTitle => "Gun Range";

        // Point shop setup
        public static readonly Vector3 PointShopPosition = new(2f, 0f, -18.5f);
        public static readonly Quaternion PointShopRotation = Quaternion.Euler(0f, -90f, 0f);

        protected override void MainSceneInitialized()
        {
            // Point shop
            PointShopHelper.SpawnBitMart(PointShopPosition, PointShopRotation);
        }
    }
}
