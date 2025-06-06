using LabFusion.Entities;
using LabFusion.Marrow.Integration;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.SDK.Extenders;
using LabFusion.Scene;

namespace LabFusion.Network;

public static class RPCIntSender
{
    public static bool SetValue(RPCInt rpcInt, int value) 
    {
        // Make sure we have a server
        if (!NetworkInfo.HasServer)
        {
            return false;
        }

        // Get the rpc event
        var hashData = RPCVariable.HashTable.GetDataFromComponent(rpcInt);

        var hasNetworkEntity = false;
        ushort entityId = 0;
        ushort componentIndex = 0;

        if (RPCVariableExtender.Cache.TryGet(rpcInt, out var entity))
        {
            // If we need ownership, make sure we have it
            if (rpcInt.RequiresOwnership && !entity.IsOwner)
            {
                return false;
            }

            hasNetworkEntity = true;
            var extender = entity.GetExtender<RPCVariableExtender>();

            entityId = entity.ID;
            componentIndex = extender.GetIndex(rpcInt).Value;
        }
        else if (rpcInt.RequiresOwnership && !NetworkInfo.IsHost)
        {
            return false;
        }

        // Send the message
        var pathData = ComponentPathData.Create(hasNetworkEntity, entityId, componentIndex, hashData);
        var intData = RPCIntData.Create(pathData, value);

        MessageRelay.RelayNative(intData, NativeMessageTag.RPCInt, NetworkChannel.Reliable, RelayType.ToClients);

        return true;
    }

    public static void CatchupValue(RPCInt rpcInt, PlayerID playerId)
    {
        // Make sure we are the level host
        if (!NetworkSceneManager.IsLevelHost)
        {
            return;
        }

        // Get the rpc event
        var hashData = RPCVariable.HashTable.GetDataFromComponent(rpcInt);

        var hasNetworkEntity = false;
        ushort entityId = 0;
        ushort componentIndex = 0;

        if (RPCVariableExtender.Cache.TryGet(rpcInt, out var entity))
        {
            hasNetworkEntity = true;
            var extender = entity.GetExtender<RPCVariableExtender>();

            entityId = entity.ID;
            componentIndex = extender.GetIndex(rpcInt).Value;
        }

        // Send the message
        var pathData = ComponentPathData.Create(hasNetworkEntity, entityId, componentIndex, hashData);
        var boolData = RPCIntData.Create(pathData, rpcInt.GetLatestValue());

        MessageRelay.RelayNative(boolData, NativeMessageTag.RPCInt, NetworkChannel.Reliable, RelayType.ToTarget, playerId.SmallID);
    }
}

[Net.SkipHandleWhileLoading]
public class RPCIntData : INetSerializable
{
    public ComponentPathData pathData;
    public int value;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref pathData);
        serializer.SerializeValue(ref value);
    }

    public static RPCIntData Create(ComponentPathData pathData, int value)
    {
        return new RPCIntData()
        {
            pathData = pathData,
            value = value,
        };
    }
}


public class RPCIntMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.RPCInt;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<RPCIntData>();

        // Entity object
        if (data.pathData.HasEntity)
        {
            var entity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.pathData.EntityId);

            if (entity == null)
            {
                return;
            }

            var extender = entity.GetExtender<RPCVariableExtender>();

            if (extender == null)
            {
                return;
            }

            var rpcVariable = extender.GetComponent(data.pathData.ComponentIndex);

            if (rpcVariable == null)
            {
                return;
            }

            var rpcInt = rpcVariable.TryCast<RPCInt>();

            if (rpcInt == null)
            {
                return;
            }

            OnFoundRPCInt(rpcInt, data.value);
        }
        // Scene object
        else
        {
            var rpcVariable = RPCVariable.HashTable.GetComponentFromData(data.pathData.HashData);

            if (rpcVariable == null)
            {
                return;
            }

            var rpcInt = rpcVariable.TryCast<RPCInt>();

            if (rpcInt == null)
            {
                return;
            }

            OnFoundRPCInt(rpcInt, data.value);
        }
    }

    private static void OnFoundRPCInt(RPCInt rpcInt, int value)
    {
        rpcInt.ReceiveValue(value);
    }
}