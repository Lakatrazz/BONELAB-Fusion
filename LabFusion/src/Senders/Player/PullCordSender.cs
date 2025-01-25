using LabFusion.Network;
using LabFusion.Player;

namespace LabFusion.Senders;

public static class PullCordSender
{
    public static void SendBodyLogToggle(bool isEnabled)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var data = BodyLogToggleData.Create(PlayerIdManager.LocalSmallId, isEnabled);

        MessageRelay.RelayNative(data, NativeMessageTag.BodyLogToggle, NetworkChannel.Reliable, RelayType.ToOtherClients);
    }

    public static void SendBodyLogEffect()
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var data = BodyLogEffectData.Create(PlayerIdManager.LocalSmallId);

        MessageRelay.RelayNative(data, NativeMessageTag.BodyLogEffect, NetworkChannel.Unreliable, RelayType.ToOtherClients);
    }
}