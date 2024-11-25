using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Marrow.Integration;

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

            entityId = entity.Id;
            componentIndex = extender.GetIndex(rpcFloat).Value;
        }
        else if (rpcFloat.RequiresOwnership && !NetworkInfo.IsServer)
        {
            return false;
        }

        // Send the message
        using var writer = FusionWriter.Create();
        var pathData = ComponentPathData.Create(hasNetworkEntity, entityId, componentIndex, hashData);
        var floatData = RPCFloatData.Create(pathData, value);

        writer.Write(floatData);

        using var message = FusionMessage.Create(NativeMessageTag.RPCFloat, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, message);

        return true;
    }
}

public class RPCFloatData : IFusionSerializable
{
    public ComponentPathData pathData;
    public float value;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(pathData);

        writer.Write(value);
    }

    public void Deserialize(FusionReader reader)
    {
        pathData = reader.ReadFusionSerializable<ComponentPathData>();

        value = reader.ReadSingle();
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


public class RPCFloatMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.RPCFloat;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        // If we are the server, broadcast the message to all clients
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(NativeMessageTag.RPCFloat, bytes);
            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
            return;
        }

        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<RPCFloatData>();

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
            var rpcVariable = RPCVariable.HashTable.GetComponentFromData(data.pathData.hashData);

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