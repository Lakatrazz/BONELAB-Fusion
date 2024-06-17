using LabFusion.Data;
using LabFusion.Patching;

namespace LabFusion.Network
{
    public enum DescentIntroType
    {
        UNKNOWN = 0,
        SEQUENCE = 1,
        BUTTON_CONFIRM = 2,
        CONFIRM_FORCE_GRAB = 3,
    }

    public class DescentIntroData : IFusionSerializable
    {
        public byte smallId;
        public byte selectionNumber;
        public DescentIntroType type;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(selectionNumber);
            writer.Write((byte)type);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            selectionNumber = reader.ReadByte();
            type = (DescentIntroType)reader.ReadByte();
        }

        public static DescentIntroData Create(byte smallId, byte selectionNumber, DescentIntroType type)
        {
            return new DescentIntroData()
            {
                smallId = smallId,
                selectionNumber = selectionNumber,
                type = type,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class DescentIntroMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.DescentIntro;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using FusionReader reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<DescentIntroData>();
            // Send message to other clients if server
            if (isServerHandled)
            {
                using var message = FusionMessage.Create(Tag.Value, bytes);
                MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
            }
            else
            {
                GameControl_DescentPatches.IgnorePatches = true;
                Control_UI_BodyMeasurementsPatches.IgnorePatches = true;

                switch (data.type)
                {
                    default:
                    case DescentIntroType.UNKNOWN:
                        break;
                    case DescentIntroType.SEQUENCE:
                        DescentData.GameController.SEQUENCE(data.selectionNumber);
                        break;
                    case DescentIntroType.BUTTON_CONFIRM:
                        DescentData.BodyMeasurementsUI.BUTTON_CONFIRM();
                        break;
                    case DescentIntroType.CONFIRM_FORCE_GRAB:
                        DescentData.GameController.CONFIRMFORCEGRAB();
                        break;
                }

                GameControl_DescentPatches.IgnorePatches = false;
                Control_UI_BodyMeasurementsPatches.IgnorePatches = false;
            }
        }
    }
}
