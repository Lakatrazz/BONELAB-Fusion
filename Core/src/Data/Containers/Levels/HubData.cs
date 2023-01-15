using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Utilities;

using SLZ.Bonelab;
using SLZ.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public static class HubData {
        public static GameControl_Hub GameController;
        public static FunicularController Funicular;

        public static void OnCacheInfo() {
            GameController = GameObject.FindObjectOfType<GameControl_Hub>(true);
            Funicular = GameObject.FindObjectOfType<FunicularController>(true);

            if (NetworkInfo.IsServer && Funicular != null) {
                PropSender.SendPropCreation(Funicular.gameObject);
            }
        }
    }
}
