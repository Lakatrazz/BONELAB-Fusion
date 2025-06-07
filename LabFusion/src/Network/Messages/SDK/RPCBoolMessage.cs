using LabFusion.Entities;
using LabFusion.Marrow.Integration;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.SDK.Extenders;

namespace LabFusion.Network;

public static class RPCBoolSender
{
    public static bool SetValue(RPCBool rpcBool, bool value)
    {
        // Make sure we have a server
        if (!NetworkInfo.HasServer)
        {
            return false;
        }

        // Get the rpc event
        var hashData = RPCVariable.HashTable.GetDataFromComponent(rpcBool);

        var hasNetworkEntity = false;
        ushort entityId = 0;
        ushort componentIndex = 0;

        if (RPCVariableExtender.Cache.TryGet(rpcBool, out var entity))
        {
            // If we need ownership, make sure we have it
            if (rpcBool.RequiresOwnership && !entity.IsOwner)
            {
                return false;
            }

            hasNetworkEntity = true;
            var extender = entity.GetExtender<RPCVariableExtender>();

            entityId = entity.ID;
            componentIndex = extender.GetIndex(rpcBool).Value;
        }
        else if (rpcBool.RequiresOwnership && !NetworkInfo.IsHost)
        {
            return false;
        }

        // Send the message
        var pathData = ComponentPathData.Create(hasNetworkEntity, entityId, componentIndex, hashData);
        var boolData = RPCBoolData.Create(pathData, value);

        MessageRelay.RelayNative(boolData, NativeMessageTag.RPCBool, NetworkChannel.Reliable, RelayType.ToClients);

        return true;
    }

    public static void CatchupValue(RPCBool rpcBool, PlayerID playerId)
    {
        // Make sure we are the level host
        if (!NetworkSceneManager.IsLevelHost)
        {
            return;
        }

        // Get the rpc event
        var hashData = RPCVariable.HashTable.GetDataFromComponent(rpcBool);

        var hasNetworkEntity = false;
        ushort entityId = 0;
        ushort componentIndex = 0;

        if (RPCVariableExtender.Cache.TryGet(rpcBool, out var entity))
        {
            hasNetworkEntity = true;
            var extender = entity.GetExtender<RPCVariableExtender>();

            entityId = entity.ID;
            componentIndex = extender.GetIndex(rpcBool).Value;
        }

        // Send the message
        var pathData = ComponentPathData.Create(hasNetworkEntity, entityId, componentIndex, hashData);
        var boolData = RPCBoolData.Create(pathData, rpcBool.GetLatestValue());

        MessageRelay.RelayNative(boolData, NativeMessageTag.RPCBool, NetworkChannel.Reliable, RelayType.ToTarget, playerId.SmallID);
    }
}

[Net.SkipHandleWhileLoading]
public class RPCBoolData : INetSerializable
{
    public ComponentPathData pathData;
    public bool value;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref pathData);
        serializer.SerializeValue(ref value);
    }

    public static RPCBoolData Create(ComponentPathData pathData, bool value)
    {
        return new RPCBoolData()
        {
            pathData = pathData,
            value = value,
        };
    }
}


public class RPCBoolMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.RPCBool;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<RPCBoolData>();

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

            var rpcBool = rpcVariable.TryCast<RPCBool>();

            if (rpcBool == null)
            {
                return;
            }

            OnFoundRPCBool(rpcBool, data.value);
        }
        // Scene object
        else
        {
            var rpcVariable = RPCVariable.HashTable.GetComponentFromData(data.pathData.HashData);

            if (rpcVariable == null)
            {
                return;
            }

            var rpcBool = rpcVariable.TryCast<RPCBool>();

            if (rpcBool == null)
            {
                return;
            }

            OnFoundRPCBool(rpcBool, data.value);
        }
    }

    private static void OnFoundRPCBool(RPCBool rpcBool, bool value)
    {
        rpcBool.ReceiveValue(value);
    }
}