using LabFusion.Senders;
using LabFusion.Utilities;
using SLZ.Bonelab;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public static class SprintBridgeData {
        public static GameControl_SprintBridge04 GameController;

        public static void OnCacheInfo() {
            GameController = GameObject.FindObjectOfType<GameControl_SprintBridge04>();

            if (GameController != null) {
                var copter = GameObject.Find("TrashCopter");

                if (copter != null) {
                    PropSender.SendPropCreation(copter);
                }
                else {
#if DEBUG
                    FusionLogger.Warn("Failed to find TrashCopter in Sprint Bridge!");
#endif
                }
            }
        }
    }
}
