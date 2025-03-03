using LabFusion.Entities;
using LabFusion.Marrow.Integration;
using LabFusion.Network.Serialization;

using UnityEngine;

namespace LabFusion.Network;

public static class RPCVector3Sender
{
    public static bool SetValue(RPCVector3 rpcVector3, Vector3 value) 
    {
        // Make sure we have a server
        if (!NetworkInfo.HasServer)
        {
            return false;
        }

        // Get the rpc event
        var hashData = RPCVariable.HashTable.GetDataFromComponent(rpcVector3);

        var hasNetworkEntity = false;
        ushort entityId = 0;
        ushort componentIndex = 0;

        if (RPCVariableExtender.Cache.TryGet(rpcVector3, out var entity))
        {
            // If we need ownership, make sure we have it
            if (rpcVector3.RequiresOwnership && !entity.IsOwner)
            {
                return false;
            }

            hasNetworkEntity = true;
            var extender = entity.GetExtender<RPCVariableExtender>();

            entityId = entity.Id;
            componentIndex = extender.GetIndex(rpcVector3).Value;
        }
        else if (rpcVector3.RequiresOwnership && !NetworkInfo.IsServer)
        {
            return false;
        }

        // Send the message
        var pathData = ComponentPathData.Create(hasNetworkEntity, entityId, componentIndex, hashData);
        var vector3Data = RPCVector3Data.Create(pathData, value);

        MessageRelay.RelayNative(vector3Data, NativeMessageTag.RPCVector3, NetworkChannel.Reliable, RelayType.ToClients);

        return true;
    }
}

public class RPCVector3Data : INetSerializable
{
    public ComponentPathData pathData;
    public Vector3 value;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref pathData);
        serializer.SerializeValue(ref value);
    }

    public static RPCVector3Data Create(ComponentPathData pathData, Vector3 value)
    {
        return new RPCVector3Data()
        {
            pathData = pathData,
            value = value,
        };
    }
}


public class RPCVector3Message : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.RPCVector3;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<RPCVector3Data>();

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

            var rpcVector3 = rpcVariable.TryCast<RPCVector3>();

            if (rpcVector3 == null)
            {
                return;
            }

            OnFoundRPCVector3(rpcVector3, data.value);
        }
        // Scene object
        else
        {
            var rpcVariable = RPCVariable.HashTable.GetComponentFromData(data.pathData.HashData);

            if (rpcVariable == null)
            {
                return;
            }

            var rpcVector3 = rpcVariable.TryCast<RPCVector3>();

            if (rpcVector3 == null)
            {
                return;
            }

            OnFoundRPCVector3(rpcVector3, data.value);
        }
    }

    private static void OnFoundRPCVector3(RPCVector3 rpcVector3, Vector3 value)
    {
        rpcVector3.ReceiveValue(value);
    }
}