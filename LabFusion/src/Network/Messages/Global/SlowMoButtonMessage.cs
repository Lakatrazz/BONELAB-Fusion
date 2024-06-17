using Il2CppSLZ.Bonelab;
using LabFusion.Data;
using LabFusion.Patching;

namespace LabFusion.Network
{
    public class SlowMoButtonMessageData : IFusionSerializable
    {
        public const int Size = sizeof(byte) * 2;

        public byte smallId;
        public bool isDecrease;

        public static SlowMoButtonMessageData Create(byte smallId, bool isDecrease)
        {
            return new SlowMoButtonMessageData()
            {
                smallId = smallId,
                isDecrease = isDecrease
            };
        }

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(isDecrease);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            isDecrease = reader.ReadBoolean();
        }
    }

    [Net.SkipHandleWhileLoading]
    public class SlowMoButtonMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.SlowMoButton;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using var reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<SlowMoButtonMessageData>();
            if (isServerHandled)
            {
                using var message = FusionMessage.Create(Tag.Value, bytes);
                MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
            }
            else
            {
                Control_GlobalTimePatches.IgnorePatches = true;

                if (data.isDecrease)
                    TimeManager.DECREASE_TIMESCALE();
                else
                    TimeManager.TOGGLE_TIMESCALE();

                Control_GlobalTimePatches.IgnorePatches = false;
            }
        }
    }
}
