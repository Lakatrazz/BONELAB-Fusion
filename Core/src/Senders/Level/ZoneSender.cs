using LabFusion.Network;

using SLZ.Zones;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Senders {
    public static class ZoneSender {
        public static void SendZoneEncounterEvent(ZoneEncounter zoneEncounter, ZoneEncounterEventType type) {
            if (NetworkInfo.IsServer) {
                using (var writer = FusionWriter.Create())
                {
                    using (var data = ZoneEncounterEventData.Create(zoneEncounter, type))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.ZoneEncounterEvent, writer))
                        {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }
    }
}
