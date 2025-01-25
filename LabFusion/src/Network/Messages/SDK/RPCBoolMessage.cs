using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Marrow.Integration;

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

            entityId = entity.Id;
            componentIndex = extender.GetIndex(rpcBool).Value;
        }
        else if (rpcBool.RequiresOwnership && !NetworkInfo.IsServer)
        {
            return false;
        }

        // Send the message
        var pathData = ComponentPathData.Create(hasNetworkEntity, entityId, componentIndex, hashData);
        var boolData = RPCBoolData.Create(pathData, value);

        MessageRelay.RelayNative(boolData, NativeMessageTag.RPCBool, NetworkChannel.Reliable, RelayType.ToClients);

        return true;
    }
}

public class RPCBoolData : IFusionSerializable
{
    public ComponentPathData pathData;
    public bool value;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(pathData);

        writer.Write(value);
    }

    public void Deserialize(FusionReader reader)
    {
        pathData = reader.ReadFusionSerializable<ComponentPathData>();

        value = reader.ReadBoolean();
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
            var rpcVariable = RPCVariable.HashTable.GetComponentFromData(data.pathData.hashData);

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