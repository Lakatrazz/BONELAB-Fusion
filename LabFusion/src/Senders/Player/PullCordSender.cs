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

        using var writer = FusionWriter.Create(BodyLogToggleData.Size);
        var data = BodyLogToggleData.Create(PlayerIdManager.LocalSmallId, isEnabled);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.BodyLogToggle, writer);
        MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
    }

    public static void SendBodyLogEffect()
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        using var writer = FusionWriter.Create(BodyLogEffectData.Size);
        var data = BodyLogEffectData.Create(PlayerIdManager.LocalSmallId);
        writer.Write(data);

        // Effects dont have to be reliable
        using var message = FusionMessage.Create(NativeMessageTag.BodyLogEffect, writer);
        MessageSender.BroadcastMessage(NetworkChannel.Unreliable, message);
    }
}