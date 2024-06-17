using LabFusion.Exceptions;

namespace LabFusion.Network;

public class EntityOwnershipRequestMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.EntityOwnershipRequest;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        if (!isServerHandled)
        {
            throw new ExpectedServerException();
        }

        // Read request
        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<EntityPlayerData>();

        // Write and send response
        using var writer = FusionWriter.Create(EntityPlayerData.Size);
        var response = EntityPlayerData.Create(data.playerId, data.entityId);
        writer.Write(response);

        using var message = FusionMessage.Create(NativeMessageTag.EntityOwnershipResponse, writer);
        MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
    }
}