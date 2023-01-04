using LabFusion.Network;
using LabFusion.Representation;

using SLZ.Zones;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Senders {
    public static class ZoneSender {
        public static void SendZoneEncounterEvent(ZoneEncounterEventType type, ZoneEncounter encounter)
        {
            using (var writer = FusionWriter.Create())
            {
                using (var data = ZoneEncounterEventData.Create(type, encounter))
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
