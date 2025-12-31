using LabFusion.Entities;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class EntityCullStatusData : INetSerializable
{
    public NetworkEntityReference Entity;
    public bool IsCulled;

    public int? GetSize() => NetworkEntityReference.Size + sizeof(bool);

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Entity);
        serializer.SerializeValue(ref IsCulled);
    }

    public NetworkEntity GetEntity() => Entity.GetEntity();
}

[Net.SkipHandleWhileLoading]
public class EntityCullStatusMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.EntityCullStatus;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var sender = received.Sender;

        if (!sender.HasValue)
        {
            return;
        }

        var data = received.ReadData<EntityCullStatusData>();

        // Find the network entity
        var entity = data.GetEntity();

        // Validate the entity
        if (entity == null || !entity.IsRegistered || entity.OwnerID == null || entity.OwnerID != sender.Value)
        {
            return;
        }

        // Get the network prop so we can update its cull status
        var networkProp = entity.GetExtender<NetworkProp>();

        if (networkProp == null)
        {
            return;
        }

        networkProp.OnReceiveCullStatus(data.IsCulled);
    }
}