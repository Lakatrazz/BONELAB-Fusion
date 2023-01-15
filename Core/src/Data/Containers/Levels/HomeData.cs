using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using SLZ.Bonelab;
using SLZ.Vehicle;
using SLZ;
using SLZ.Marrow.VoidLogic;

using LabFusion.Utilities;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Senders;

namespace LabFusion.Data
{
    public static class HomeData
    {
        public static GameControl_Outro GameController;
        public static TaxiController TaxiController;
        public static Seat TaxiSeat;
        public static ArticulatedArmController ArmController;
        public static ArmFinale ArmFinale;

        public static void OnCacheInfo()
        {
            GameController = GameObject.FindObjectOfType<GameControl_Outro>(true);
            if (GameController != null) {
                TaxiController = GameObject.FindObjectOfType<TaxiController>(true);
                TaxiSeat = TaxiController.rearSeat;
                ArmController = GameObject.FindObjectOfType<ArticulatedArmController>(true);
                ArmFinale = GameObject.FindObjectOfType<ArmFinale>(true);

                // Add extra seats
                // Inside seat
                Internal_CreateSeat(2, new Vector3(-0.326f, 0.441f, -1.125f), Vector3.zero);

                // Trunk seats
                Internal_CreateSeat(3, new Vector3(0.48f, 0.928f, -2.138f), Vector3.up * -180f);
                Internal_CreateSeat(4, new Vector3(-0.48f, 0.928f, -2.138f), Vector3.up * -180f);

                // Hood seats
                Internal_CreateSeat(5, new Vector3(0.48f, 0.928f, 1.998f), Vector3.zero);
                Internal_CreateSeat(6, new Vector3(-0.48f, 0.928f, 1.998f), Vector3.zero);

                // Send the taxi, windmill, and lift to sync
                if (NetworkInfo.IsServer) {
                    // Taxi
                    PropSender.SendPropCreation(TaxiController.gameObject, null, true);

                    // Windmill
                    var windmill = ArmFinale.transform.Find("WindMill (2)");
                    if (windmill != null)
                        PropSender.SendPropCreation(windmill.gameObject);
                    else {
#if DEBUG
                        FusionLogger.Warn("Failed to find Windmill!");
#endif
                    }
                }
            }
        }

        private static void Internal_CreateSeat(int index, Vector3 localPosition, Vector3 localRotation) {
            var extraSeat = GameObject.Instantiate(TaxiSeat.gameObject);
            extraSeat.transform.parent = TaxiSeat.transform.parent;
            extraSeat.SetActive(true);
            extraSeat.name = $"Seat ({index})";

            extraSeat.transform.localPosition = localPosition;
            extraSeat.transform.localRotation = Quaternion.Euler(localRotation);
        }

        public static void TeleportToJimmyFinger() {
            var rm = RigData.RigReferences.RigManager;
            if (!rm.IsNOC()) {
                var pos = new Vector3(-0.25f, 95.23f, 13f);

                rm.Teleport(pos, true);
                rm.physicsRig.ResetHands(Handedness.BOTH);
            }
        }
    }
}
