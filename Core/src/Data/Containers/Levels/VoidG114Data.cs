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

namespace LabFusion.Data {
    public static class VoidG114Data {
        public static GameControl_MenuVoidG114 GameController;

        public static readonly Vector3 PointShopPosition = new Vector3(25.4318f, -1.1765f, 1.8907f);
        public static readonly Quaternion PointShopRotation = Quaternion.Euler(0f, 90f, 0f);

        public static readonly Vector3 SupportCubePosition = new Vector3(25.5273f, -1.6745f, 2.1946f);
        public static readonly Quaternion SupportCubeRotation = Quaternion.Euler(0f, 0f, 0f);
        public static readonly Vector3 SupportCubeScale = new Vector3(2.04f, 1f, 2.74f);

        public static void OnCacheInfo() {
            GameController = GameObject.FindObjectOfType<GameControl_MenuVoidG114>(true);

            if (GameController != null) {
                PointShopHelper.SetupPointShop(PointShopPosition, PointShopRotation, Vector3.one * 0.8f);

                GameObject supportCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                supportCube.name = "Point Shop Support Cube";
                supportCube.transform.position = SupportCubePosition;
                supportCube.transform.rotation = SupportCubeRotation;
                supportCube.transform.localScale = SupportCubeScale;

                var meshRenderer = supportCube.GetComponentInChildren<MeshRenderer>();
                var planeRenderer = GameObject.Find("plane_1x6").GetComponentInChildren<MeshRenderer>();

                meshRenderer.material = planeRenderer.material;
            }
        }
    }
}
