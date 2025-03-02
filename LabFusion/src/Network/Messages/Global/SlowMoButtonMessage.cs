using Il2CppSLZ.Marrow;

using LabFusion.Network.Serialization;
using LabFusion.Patching;

namespace LabFusion.Network;

public class SlowMoButtonMessageData : INetSerializable
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

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref isDecrease);
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
