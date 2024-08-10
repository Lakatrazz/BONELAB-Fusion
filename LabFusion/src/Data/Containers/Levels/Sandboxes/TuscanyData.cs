using UnityEngine;

using LabFusion.Extensions;
using LabFusion.SDK.Points;

namespace LabFusion.Data
{
    public class TuscanyData : LevelDataHandler
    {
        public override string LevelTitle => "Tuscany";

        // Point shop setup
        public static readonly Vector3 PointShopPosition = new(-9f, 0f, 8f);
        public static readonly Quaternion PointShopRotation = Quaternion.Euler(0f, -90f, 0f);

        protected override void MainSceneInitialized()
        {
            // Point shop
            PointShopHelper.SpawnBitMart(PointShopPosition, PointShopRotation);
        }
    }
}
