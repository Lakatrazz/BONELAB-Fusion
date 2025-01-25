namespace LabFusion.Network;

public class EntityOwnershipRequestMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.EntityOwnershipRequest;

    public override ExpectedType ExpectedReceiver => ExpectedType.ServerOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        // Read request
        var data = received.ReadData<EntityPlayerData>();

        // Send response
        var response = EntityPlayerData.Create(data.playerId, data.entityId);

        MessageRelay.RelayNative(response, NativeMessageTag.EntityOwnershipResponse, NetworkChannel.Reliable, RelayType.ToClients);
    }
}