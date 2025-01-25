using Il2CppSLZ.Marrow;

using LabFusion.Data;
using LabFusion.Patching;

namespace LabFusion.Network;

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
public class SlowMoButtonMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.SlowMoButton;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<SlowMoButtonMessageData>();

        TimeManagerPatches.IgnorePatches = true;

        if (data.isDecrease)
        {
            TimeManager.DECREASE_TIMESCALE();
        }
        else
        {
            TimeManager.TOGGLE_TIMESCALE();
        }

        TimeManagerPatches.IgnorePatches = false;
    }
}
