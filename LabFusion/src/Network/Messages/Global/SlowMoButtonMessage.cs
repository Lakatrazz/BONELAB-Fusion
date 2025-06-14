using Il2CppSLZ.Marrow;

using LabFusion.Network.Serialization;
using LabFusion.Patching;
using LabFusion.Player;
using LabFusion.Preferences;
using LabFusion.Senders;

namespace LabFusion.Network;

public class SlowMoButtonMessageData : INetSerializable
{
    public int? GetSize() => sizeof(byte);

    public bool Decrease;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Decrease);
    }
}

[Net.SkipHandleWhileLoading]
public class SlowMoButtonMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.SlowMoButton;

    protected override bool OnPreRelayMessage(ReceivedMessage received)
    {
        var mode = CommonPreferences.SlowMoMode;

        return mode switch
        {
            TimeScaleMode.DISABLED => false,
            TimeScaleMode.LOW_GRAVITY => false,
            TimeScaleMode.CLIENT_SIDE => false,
            TimeScaleMode.HOST_ONLY => received.Sender == PlayerIDManager.HostSmallID,
            _ => true,
        };
    }

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<SlowMoButtonMessageData>();

        TimeManagerPatches.IgnorePatches = true;

        if (data.Decrease)
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
