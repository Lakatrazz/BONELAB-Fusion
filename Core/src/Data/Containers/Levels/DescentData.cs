using LabFusion.Network;
using LabFusion.Representation;
using SLZ.Bonelab;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public static class DescentData {
        public static NooseBonelabIntro Noose;
        public static TutorialElevator Elevator;

        public static void TEMP_SendElevator(DescentElevatorType type)
        {
            using (var writer = FusionWriter.Create())
            {
                using (var data = DescentElevatorData.Create(PlayerIdManager.LocalSmallId, type))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.DescentElevator, writer))
                    {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void OnCacheDescentInfo() {
            Noose = GameObject.FindObjectOfType<NooseBonelabIntro>(true);
            Elevator = GameObject.FindObjectOfType<TutorialElevator>(true);
        }
    }
}
