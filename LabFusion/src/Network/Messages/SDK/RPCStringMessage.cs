using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Marrow.Integration;

namespace LabFusion.Network;

public static class RPCStringSender
{
    public static bool SetValue(RPCString rpcString, string value) 
    {
        // Make sure we have a server
        if (!NetworkInfo.HasServer)
        {
            return false;
        }

        // Get the rpc event
        var hashData = RPCVariable.HashTable.GetDataFromComponent(rpcString);

        var hasNetworkEntity = false;
        ushort entityId = 0;
        ushort componentIndex = 0;

        if (RPCVariableExtender.Cache.TryGet(rpcString, out var entity))
        {
            // If we need ownership, make sure we have it
            if (rpcString.RequiresOwnership && !entity.IsOwner)
            {
                return false;
            }

            hasNetworkEntity = true;
            var extender = entity.GetExtender<RPCVariableExtender>();

            entityId = entity.Id;
            componentIndex = extender.GetIndex(rpcString).Value;
        }
        else if (rpcString.RequiresOwnership && !NetworkInfo.IsServer)
        {
            return false;
        }

        // Send the message
        var pathData = ComponentPathData.Create(hasNetworkEntity, entityId, componentIndex, hashData);
        var stringData = RPCStringData.Create(pathData, value);

        MessageRelay.RelayNative(stringData, NativeMessageTag.RPCString, NetworkChannel.Reliable, RelayType.ToClients);

        return true;
    }
}

public class RPCStringData : IFusionSerializable
{
    public ComponentPathData pathData;
    public string value;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(pathData);

        writer.Write(value);
    }

    public void Deserialize(FusionReader reader)
    {
        pathData = reader.ReadFusionSerializable<ComponentPathData>();

        value = reader.ReadString();
    }

    public static RPCStringData Create(ComponentPathData pathData, string value)
    {
        return new RPCStringData()
        {
            pathData = pathData,
            value = value,
        };
    }
}


public class RPCStringMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.RPCString;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<RPCStringData>();

        // Entity object
        if (data.pathData.hasNetworkEntity)
        {
            var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.pathData.entityId);

            if (entity == null)
            {
                return;
            }

            var extender = entity.GetExtender<RPCVariableExtender>();

            if (extender == null)
            {
                return;
            }

            var rpcVariable = extender.GetComponent(data.pathData.componentIndex);

            if (rpcVariable == null)
            {
                return;
            }

            var rpcString = rpcVariable.TryCast<RPCString>();

            if (rpcString == null)
            {
                return;
            }

            OnFoundRPCString(rpcString, data.value);
        }
        // Scene object
        else
        {
            var rpcVariable = RPCVariable.HashTable.GetComponentFromData(data.pathData.hashData);

            if (rpcVariable == null)
            {
                return;
            }

            var rpcString = rpcVariable.TryCast<RPCString>();

            if (rpcString == null)
            {
                return;
            }

            OnFoundRPCString(rpcString, data.value);
        }
    }

    private static void OnFoundRPCString(RPCString rpcString, string value)
    {
        rpcString.ReceiveValue(value);
    }
}