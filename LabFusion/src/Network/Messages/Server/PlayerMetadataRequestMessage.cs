using LabFusion.Senders;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class PlayerMetadataRequestMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerMetadataRequest;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ServerOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerMetadataData>();

        // Make sure the message sender is able to modify this player's metadata
        if (!NetworkVerification.HasAuthorityOverPlayer(data.Player.ID, received.Sender))
        {
            var descriptor = string.IsNullOrEmpty(received.PlatformID) ? $"{received.PlatformID}" : "with no PlatformID";
            FusionLogger.Warn($"User {descriptor} attempted to modify metadata for player {data.Player.ID}!");
            return;
        }

        // Send the response to all clients
        PlayerSender.SendPlayerMetadataResponse(data.Player.ID, data.Key, data.Value);
    }
}
