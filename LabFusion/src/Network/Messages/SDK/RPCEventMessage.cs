using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Marrow.Integration;

namespace LabFusion.Network;

public static class RPCEventSender
{
    public static bool Invoke(RPCEvent rpcEvent) 
    {
        // Make sure we have a server
        if (!NetworkInfo.HasServer)
        {
            return false;
        }

        var target = (RPCEvent.RPCTarget)rpcEvent.target.Get();

        // If the target is to clients, but we aren't the server, we can't send the message
        if (target == RPCEvent.RPCTarget.Clients && !NetworkInfo.IsServer)
        {
            return false;
        }

        var channel = (RPCEvent.RPCChannel)rpcEvent.channel.Get();

        // Convert to network channel
        var networkChannel = channel == RPCEvent.RPCChannel.Reliable ? NetworkChannel.Reliable : NetworkChannel.Unreliable;

        // Get the rpc event
        var hashData = RPCEvent.HashTable.GetDataFromComponent(rpcEvent);

        var hasNetworkEntity = false;
        ushort entityId = 0;
        ushort componentIndex = 0;

        if (RPCEventExtender.Cache.TryGet(rpcEvent, out var entity))
        {
            // If we need ownership, make sure we have it
            if (rpcEvent.requiresOwnership.Get() && !entity.IsOwner)
            {
                return false;
            }

            hasNetworkEntity = true;
            var extender = entity.GetExtender<RPCEventExtender>();

            entityId = entity.Id;
            componentIndex = extender.GetIndex(rpcEvent).Value;
        }

        // Send the message
        using var writer = FusionWriter.Create();
        var data = RPCEventData.Create(hasNetworkEntity, entityId, componentIndex, hashData);

        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.RPCEvent, writer);

        switch (target)
        {
            case RPCEvent.RPCTarget.Server:
                MessageSender.SendToServer(networkChannel, message);
                return true;
            case RPCEvent.RPCTarget.Clients:
                MessageSender.BroadcastMessage(networkChannel, message);
                return true;
        }

        // Target wasn't valid?
        return false;
    }
}

public class RPCEventData : IFusionSerializable
{
    public bool hasNetworkEntity;

    public ushort entityId;
    public ushort componentIndex;

    public ComponentHashData hashData;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(hasNetworkEntity);

        writer.Write(entityId);
        writer.Write(componentIndex);

        writer.Write(hashData);
    }

    public void Deserialize(FusionReader reader)
    {
        hasNetworkEntity = reader.ReadBoolean();

        entityId = reader.ReadUInt16();
        componentIndex = reader.ReadUInt16();

        hashData = reader.ReadFusionSerializable<ComponentHashData>();
    }

    public static RPCEventData Create(bool hasNetworkEntity, ushort entityId, ushort componentIndex, ComponentHashData hashData)
    {
        return new RPCEventData()
        {
            hasNetworkEntity = hasNetworkEntity,
            entityId = entityId,
            componentIndex = componentIndex,
            hashData = hashData,
        };
    }
}

public class RPCEventMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.RPCEvent;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<RPCEventData>();

        // Entity object
        if (data.hasNetworkEntity)
        {
            var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.entityId);

            if (entity == null)
            {
                return;
            }

            var extender = entity.GetExtender<RPCEventExtender>();

            if (extender == null)
            {
                return;
            }

            var rpcEvent = extender.GetComponent(data.componentIndex);

            if (rpcEvent == null)
            {
                return;
            }

            OnFoundRPCEvent(rpcEvent);
        }
        // Scene object
        else
        {
            var rpcEvent = RPCEvent.HashTable.GetComponentFromData(data.hashData);

            if (rpcEvent == null)
            {
                return;
            }

            OnFoundRPCEvent(rpcEvent);
        }
    }

    private void OnFoundRPCEvent(RPCEvent rpcEvent)
    {
        rpcEvent.Receive();
    }
}