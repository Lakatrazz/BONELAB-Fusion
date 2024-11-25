using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Marrow.Integration;

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

            entityId = entity.Id;
            componentIndex = extender.GetIndex(rpcInt).Value;
        }
        else if (rpcInt.RequiresOwnership && !NetworkInfo.IsServer)
        {
            return false;
        }

        // Send the message
        using var writer = FusionWriter.Create();
        var pathData = ComponentPathData.Create(hasNetworkEntity, entityId, componentIndex, hashData);
        var intData = RPCIntData.Create(pathData, value);

        writer.Write(intData);

        using var message = FusionMessage.Create(NativeMessageTag.RPCInt, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, message);

        return true;
    }
}

public class RPCIntData : IFusionSerializable
{
    public ComponentPathData pathData;
    public int value;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(pathData);

        writer.Write(value);
    }

    public void Deserialize(FusionReader reader)
    {
        pathData = reader.ReadFusionSerializable<ComponentPathData>();

        value = reader.ReadInt32();
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


public class RPCIntMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.RPCInt;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        // If we are the server, broadcast the message to all clients
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(NativeMessageTag.RPCInt, bytes);
            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
            return;
        }

        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<RPCIntData>();

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
            var rpcVariable = RPCVariable.HashTable.GetComponentFromData(data.pathData.hashData);

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