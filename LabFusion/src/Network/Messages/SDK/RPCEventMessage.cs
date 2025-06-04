using LabFusion.Entities;
using LabFusion.Marrow.Integration;
using LabFusion.SDK.Extenders;

namespace LabFusion.Network;

public static class RPCEventSender
{
    private static RelayType ConvertRPCRelayType(RPCEvent.RPCRelayType relayType)
    {
        return relayType switch
        {
            RPCEvent.RPCRelayType.ToClients => RelayType.ToClients,
            RPCEvent.RPCRelayType.ToOtherClients => RelayType.ToOtherClients,
            _ => RelayType.ToServer,
        };
    }

    private static NetworkChannel ConvertRPCChannel(RPCEvent.RPCChannel channel)
    {
        return channel switch
        {
            RPCEvent.RPCChannel.Reliable => NetworkChannel.Reliable,
            _ => NetworkChannel.Unreliable,
        };
    }

    public static bool Invoke(RPCEvent rpcEvent) 
    {
        // Make sure we have a server
        if (!NetworkInfo.HasServer)
        {
            return false;
        }

        var relayType = ConvertRPCRelayType(rpcEvent.RelayType);

        var channel = ConvertRPCChannel(rpcEvent.Channel);

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

            entityId = entity.ID;
            componentIndex = extender.GetIndex(rpcEvent).Value;
        }
        else if (rpcEvent.requiresOwnership && !NetworkInfo.IsHost)
        {
            return false;
        }

        // Send the message
        var data = ComponentPathData.Create(hasNetworkEntity, entityId, componentIndex, hashData);

        MessageRelay.RelayNative(data, NativeMessageTag.RPCEvent, channel, relayType);

        return true;
    }
}

[Net.DelayWhileTargetLoading]
public class RPCEventMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.RPCEvent;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ComponentPathData>();

        // Entity object
        if (data.HasEntity)
        {
            var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.EntityId);

            if (entity == null)
            {
                return;
            }

            var extender = entity.GetExtender<RPCEventExtender>();

            if (extender == null)
            {
                return;
            }

            var rpcEvent = extender.GetComponent(data.ComponentIndex);

            if (rpcEvent == null)
            {
                return;
            }

            OnFoundRPCEvent(rpcEvent);
        }
        // Scene object
        else
        {
            var rpcEvent = RPCEvent.HashTable.GetComponentFromData(data.HashData);

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