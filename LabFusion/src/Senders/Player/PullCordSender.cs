using LabFusion.Network;
using LabFusion.Representation;

namespace LabFusion.Senders
{
    public static class PullCordSender
    {
        public static void SendBodyLogToggle(bool isEnabled)
        {
            if (!NetworkInfo.HasServer)
                return;

            using var writer = FusionWriter.Create(BodyLogToggleData.Size);
            using var data = BodyLogToggleData.Create(PlayerIdManager.LocalSmallId, isEnabled);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.BodyLogToggle, writer);
            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
        }

        public static void SendBodyLogEffect()
        {
            if (!NetworkInfo.HasServer)
                return;

            using var writer = FusionWriter.Create(BodyLogEffectData.Size);
            using var data = BodyLogEffectData.Create(PlayerIdManager.LocalSmallId);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.BodyLogEffect, writer);
            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
        }
    }
}
