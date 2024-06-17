using LabFusion.Data;
using LabFusion.Patching;

namespace LabFusion.Network
{
    public enum KartRaceEventType
    {
        UNKNOWN = 0,
        START_RACE = 1,
        NEW_LAP = 2,
        RESET_RACE = 3,
        END_RACE = 4,
    }

    public class KartRaceEventData : IFusionSerializable
    {
        public byte smallId;
        public KartRaceEventType type;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write((byte)type);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            type = (KartRaceEventType)reader.ReadByte();
        }

        public static KartRaceEventData Create(byte smallId, KartRaceEventType type)
        {
            return new KartRaceEventData()
            {
                smallId = smallId,
                type = type,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class KartRaceEventMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.KartRaceEvent;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<KartRaceEventData>();
            // Send message to other clients if server
            if (isServerHandled)
            {
                using var message = FusionMessage.Create(Tag.Value, bytes);
                MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
            }
            else
            {
                KartRacePatches.IgnorePatches = true;

                switch (data.type)
                {
                    default:
                    case KartRaceEventType.UNKNOWN:
                        break;
                    case KartRaceEventType.START_RACE:
                        KartRaceData.GameController.STARTRACE();
                        break;
                    case KartRaceEventType.NEW_LAP:
                        KartRaceData.GameController.NEWLAP();
                        break;
                    case KartRaceEventType.RESET_RACE:
                        KartRaceData.GameController.RESETRACE();
                        break;
                    case KartRaceEventType.END_RACE:
                        KartRaceData.GameController.ENDRACE();
                        break;
                }

                KartRacePatches.IgnorePatches = false;
            }
        }
    }
}
