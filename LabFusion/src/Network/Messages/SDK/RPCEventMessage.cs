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

        var target = (RPCEvent.RPCTarget)rpcEvent.Target;

        // If the target is to clients, but we aren't the server, we can't send the message
        if (target == RPCEvent.RPCTarget.Clients && !NetworkInfo.IsServer)
        {
            return false;
        }

        var channel = (RPCEvent.RPCChannel)rpcEvent.Channel;

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
            if (rpcEvent.RequiresOwnership && !entity.IsOwner)
            {
                return false;
            }

            hasNetworkEntity = true;
            var extender = entity.GetExtender<RPCEventExtender>();

            entityId = entity.Id;
            componentIndex = extender.GetIndex(rpcEvent).Value;
        }
        else if (rpcEvent.requiresOwnership && !NetworkInfo.IsServer)
        {
            return false;
        }

        // Send the message
        using var writer = FusionWriter.Create();
        var data = ComponentPathData.Create(hasNetworkEntity, entityId, componentIndex, hashData);

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

public class RPCEventMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.RPCEvent;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ComponentPathData>();

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