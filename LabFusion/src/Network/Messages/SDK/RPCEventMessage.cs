using LabFusion.Marrow.Integration;
using LabFusion.Scene;
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
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return false;
        }

        var relayType = ConvertRPCRelayType(rpcEvent.RelayType);

        var channel = ConvertRPCChannel(rpcEvent.Channel);

        // Check for ownership
        if (RPCEventExtender.Cache.TryGet(rpcEvent, out var entity))
        {
            if (rpcEvent.RequiresOwnership && !entity.IsOwner)
            {
                return false;
            }
        }
        else if (rpcEvent.requiresOwnership && !NetworkInfo.IsHost)
        {
            return false;
        }

        // Send the message
        var data = ComponentPathData.CreateFromComponent<RPCEvent, RPCEventExtender>(rpcEvent, RPCEvent.HashTable, RPCEventExtender.Cache);

        MessageRelay.RelayNative(data, NativeMessageTag.RPCEvent, new MessageRoute(relayType, channel));

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

        if (data.TryGetComponent<RPCEvent, RPCEventExtender>(RPCEvent.HashTable, out var rpcEvent))
        {
            OnFoundRPCEvent(rpcEvent);
        }
    }

    private void OnFoundRPCEvent(RPCEvent rpcEvent)
    {
        rpcEvent.Receive();
    }
}