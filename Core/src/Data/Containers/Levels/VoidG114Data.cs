using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Points;

using SLZ.Bonelab;
using SLZ.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public static class VoidG114Data {
        public static GameControl_MenuVoidG114 GameController;

        public static readonly Vector3 PointShopPosition = new Vector3(30.98999f, -2.0265f, 5.037982f);
        public static readonly Quaternion PointShopRotation = Quaternion.Euler(0f, -90f, 0f);

        public static void OnCacheInfo() {
            GameController = GameObject.FindObjectOfType<GameControl_MenuVoidG114>(true);

            if (GameController != null) {
                PointShopHelper.SetupPointShop(PointShopPosition, PointShopRotation, Vector3.one);
            }
        }
    }
}
