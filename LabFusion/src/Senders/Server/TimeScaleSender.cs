using LabFusion.Network;
using LabFusion.Player;

namespace LabFusion.Senders;

public enum TimeScaleMode
{
    DISABLED = 0,
    LOW_GRAVITY = 1,
    HOST_ONLY = 2,
    EVERYONE = 3,
    CLIENT_SIDE = 4,
}

public static class TimeScaleSender
{
    public static void SendSlowMoButton(bool decrease)
    {
        MessageRelay.RelayNative(new SlowMoButtonMessageData() { Decrease = decrease }, NativeMessageTag.SlowMoButton, NetworkChannel.Reliable, RelayType.ToOtherClients);
    }
}
