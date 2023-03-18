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
    public class HolodeckData : LevelDataHandler
    {
        public static GameControl_Holodeck GameController;
        protected override void MainSceneInitialized() {
            GameController = GameObject.FindObjectOfType<GameControl_Holodeck>(true);
        }
    }
}
