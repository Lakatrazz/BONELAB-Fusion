namespace LabFusion.Network;

public class PlayerMetadataResponseMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerMetadataResponse;

    public override ExpectedSenderType ExpectedSender => ExpectedSenderType.ServerOnly;
    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerMetadataData>();

        var playerID = data.Player.GetPlayer();

        playerID?.Metadata.Metadata.ForceSetLocalMetadata(data.Key, data.Value);
    }
}