using LabFusion.Player;
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

        var key = data.Key;
        var value = data.Value;
        var playerID = data.Player.ID;

        var sender = received.Sender;

        bool hasValue = !string.IsNullOrWhiteSpace(received.PlatformID) || received.PlatformID != "0";
        
        // Make sure the message sender is able to modify this player's metadata
        if (!NetworkVerification.HasAuthorityOverPlayer(playerID, sender))
        {
            var descriptor = hasValue ? $"{received.PlatformID}" : "with no PlatformID";
            FusionLogger.Warn($"User {descriptor} attempted to modify metadata for player {playerID}!");
            return;
        }

        // If the player does not have authority over this specific metadata key, do not allow them to change it
        if (!PlayerMetadata.Processor.HasAuthorityOverKey(key, sender))
        {
            var descriptor = hasValue ? $"{received.PlatformID}" : "with no PlatformID";
            FusionLogger.Warn($"User {descriptor} attempted to modify metadata with key {key}, which they do not have authority over!");
            return;
        }

        // Send the response to all clients
        PlayerSender.SendPlayerMetadataResponse(playerID, key, value);
    }
}
