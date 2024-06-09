using LabFusion.Data;
using LabFusion.Patching;

namespace LabFusion.Network
{
    public enum MagmaGateEventType
    {
        UNKNOWN = 0,
        BUTTONS_SETUP = 1,
        OBJECTIVE_COMPLETE_SETUP = 2,
        LOSE_SEQUENCE = 3,
        DOOR_DISSOLVE = 4,
    }

    public class MagmaGateEventData : IFusionSerializable
    {
        public MagmaGateEventType type;

        public void Serialize(FusionWriter writer)
        {
            writer.Write((byte)type);
        }

        public void Deserialize(FusionReader reader)
        {
            type = (MagmaGateEventType)reader.ReadByte();
        }

        public static MagmaGateEventData Create(MagmaGateEventType type)
        {
            return new MagmaGateEventData()
            {
                type = type,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class MagmaGateEventMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.MagmaGateEvent;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<MagmaGateEventData>();
            var controller = MagmaGateData.GameController;
            MagmaGatePatches.IgnorePatches = true;

            // We ONLY handle this for clients, this message should only ever be sent by the server!
            if (!NetworkInfo.IsServer && controller)
            {
                switch (data.type)
                {
                    default:
                    case MagmaGateEventType.UNKNOWN:
                        break;
                    case MagmaGateEventType.BUTTONS_SETUP:
                        controller.LevelSetup();
                        break;
                    case MagmaGateEventType.OBJECTIVE_COMPLETE_SETUP:
                        controller.ObjectiveComplete();
                        break;
                    case MagmaGateEventType.LOSE_SEQUENCE:
                        controller.LoseSequence();
                        break;
                    case MagmaGateEventType.DOOR_DISSOLVE:
                        controller.DoorDissolve();
                        break;
                }
            }

            MagmaGatePatches.IgnorePatches = false;
        }
    }
}
