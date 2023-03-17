using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.SDK.Points;

using SLZ.Bonelab;
using SLZ.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using LabFusion.Extensions;

namespace LabFusion.Data {
    public static class HubData {
        public static GameControl_Hub GameController;
        public static FunicularController Funicular;

        public static readonly Vector3 PointShopPosition = new Vector3(-5.69f, -0.013f, 39.79f);
        public static readonly Quaternion PointShopRotation = Quaternion.Euler(0f, 90f, 0f);

        public static void OnCacheInfo() {
            GameController = GameObject.FindObjectOfType<GameControl_Hub>(true);
            Funicular = GameObject.FindObjectOfType<FunicularController>(true);

            if (GameController != null) {
                PointShopHelper.SetupPointShop(PointShopPosition, PointShopRotation, Vector3Extensions.one * 0.8f);
            }

            if (NetworkInfo.IsServer && Funicular != null) {
                PropSender.SendPropCreation(Funicular.gameObject);
            }
        }

        public static bool IsInHub() => GameController != null;
    }
}
