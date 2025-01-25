using LabFusion.Network;
using LabFusion.Player;

namespace LabFusion.Senders;

public enum TimeScaleMode
{
    DISABLED = 0,
    LOW_GRAVITY = 1,
    HOST_ONLY = 2,
    EVERYONE = 3,
    CLIENT_SIDE_UNSTABLE = 4,
}

public static class TimeScaleSender
{
    public static void SendSlowMoButton(bool isDecrease)
    {
        var data = SlowMoButtonMessageData.Create(PlayerIdManager.LocalSmallId, isDecrease);

        MessageRelay.RelayNative(data, NativeMessageTag.SlowMoButton, NetworkChannel.Reliable, RelayType.ToOtherClients);
    }
}
