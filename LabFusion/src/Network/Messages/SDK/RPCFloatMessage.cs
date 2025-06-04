using LabFusion.SDK.Extenders;
using LabFusion.Entities;
using LabFusion.Marrow.Integration;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Scene;

namespace LabFusion.Network;

public static class RPCFloatSender
{
    public static bool SetValue(RPCFloat rpcFloat, float value) 
    {
        // Make sure we have a server
        if (!NetworkInfo.HasServer)
        {
            return false;
        }

        // Get the rpc event
        var hashData = RPCVariable.HashTable.GetDataFromComponent(rpcFloat);

        var hasNetworkEntity = false;
        ushort entityId = 0;
        ushort componentIndex = 0;

        if (RPCVariableExtender.Cache.TryGet(rpcFloat, out var entity))
        {
            // If we need ownership, make sure we have it
            if (rpcFloat.RequiresOwnership && !entity.IsOwner)
            {
                return false;
            }

            hasNetworkEntity = true;
            var extender = entity.GetExtender<RPCVariableExtender>();

            entityId = entity.ID;
            componentIndex = extender.GetIndex(rpcFloat).Value;
        }
        else if (rpcFloat.RequiresOwnership && !NetworkInfo.IsHost)
        {
            return false;
        }

        // Send the message
        var pathData = ComponentPathData.Create(hasNetworkEntity, entityId, componentIndex, hashData);
        var floatData = RPCFloatData.Create(pathData, value);

        MessageRelay.RelayNative(floatData, NativeMessageTag.RPCFloat, NetworkChannel.Reliable, RelayType.ToClients);

        return true;
    }

    public static void CatchupValue(RPCFloat rpcFloat, PlayerID playerId)
    {
        // Make sure we are the level host
        if (!NetworkSceneManager.IsLevelHost)
        {
            return;
        }

        // Get the rpc event
        var hashData = RPCVariable.HashTable.GetDataFromComponent(rpcFloat);

        var hasNetworkEntity = false;
        ushort entityId = 0;
        ushort componentIndex = 0;

        if (RPCVariableExtender.Cache.TryGet(rpcFloat, out var entity))
        {
            hasNetworkEntity = true;
            var extender = entity.GetExtender<RPCVariableExtender>();

            entityId = entity.ID;
            componentIndex = extender.GetIndex(rpcFloat).Value;
        }

        // Send the message
        var pathData = ComponentPathData.Create(hasNetworkEntity, entityId, componentIndex, hashData);
        var boolData = RPCFloatData.Create(pathData, rpcFloat.GetLatestValue());

        MessageRelay.RelayNative(boolData, NativeMessageTag.RPCFloat, NetworkChannel.Reliable, RelayType.ToTarget, playerId.SmallID);
    }
}

[Net.SkipHandleWhileLoading]
public class RPCFloatData : INetSerializable
{
    public ComponentPathData pathData;
    public float value;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref pathData);
        serializer.SerializeValue(ref value);
    }

    public static RPCFloatData Create(ComponentPathData pathData, float value)
    {
        return new RPCFloatData()
        {
            pathData = pathData,
            value = value,
        };
    }
}


public class RPCFloatMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.RPCFloat;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<RPCFloatData>();

        // Entity object
        if (data.pathData.HasEntity)
        {
            var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.pathData.EntityId);

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

            var rpcFloat = rpcVariable.TryCast<RPCFloat>();

            if (rpcFloat == null)
            {
                return;
            }

            OnFoundRPCFloat(rpcFloat, data.value);
        }
        // Scene object
        else
        {
            var rpcVariable = RPCVariable.HashTable.GetComponentFromData(data.pathData.HashData);

            if (rpcVariable == null)
            {
                return;
            }

            var rpcFloat = rpcVariable.TryCast<RPCFloat>();

            if (rpcFloat == null)
            {
                return;
            }

            OnFoundRPCFloat(rpcFloat, data.value);
        }
    }

    private static void OnFoundRPCFloat(RPCFloat rpcFloat, float value)
    {
        rpcFloat.ReceiveValue(value);
    }
}