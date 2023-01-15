using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using SLZ.Bonelab;
using SLZ.Vehicle;
using SLZ;

using LabFusion.Utilities;
using LabFusion.Extensions;

namespace LabFusion.Data
{
    public static class HomeData
    {
        public static GameControl_Outro GameController;
        public static TaxiController TaxiController;
        public static Seat TaxiSeat;
        public static ArticulatedArmController ArmController;

        public static void OnCacheInfo()
        {
            GameController = GameObject.FindObjectOfType<GameControl_Outro>(true);
            if (GameController != null) {
                TaxiController = GameObject.FindObjectOfType<TaxiController>(true);
                TaxiSeat = TaxiController.rearSeat;
                ArmController = GameObject.FindObjectOfType<ArticulatedArmController>(true);

                // Add extra seat
                var extraSeat = GameObject.Instantiate(TaxiSeat.gameObject);
                extraSeat.transform.parent = TaxiSeat.transform.parent;
                extraSeat.SetActive(true);
                extraSeat.name = "Seat (2)";
                extraSeat.transform.localPosition = new Vector3(-0.326f, 0.441f, -1.125f);
            }
        }

        public static void TeleportToJimmyFinger() {
            var rm = RigData.RigReferences.RigManager;
            if (!rm.IsNOC()) {
                var pos = ArmController.index1.transform.position;

                rm.Teleport(pos, true);
                rm.physicsRig.ResetHands(Handedness.BOTH);
            }
        }
    }
}
