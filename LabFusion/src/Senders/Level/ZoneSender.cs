using LabFusion.Network;

using Il2CppSLZ.Marrow.Zones;

namespace LabFusion.Senders
{
    public static class ZoneSender
    {
        public static void SendZoneEncounterEvent(MobileEncounter zoneEncounter, ZoneEncounterEventType type)
        {
            if (NetworkInfo.IsServer)
            {
                using var writer = FusionWriter.Create();
                var data = ZoneEncounterEventData.Create(zoneEncounter, type);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.ZoneEncounterEvent, writer);
                MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
            }
        }
    }
}
