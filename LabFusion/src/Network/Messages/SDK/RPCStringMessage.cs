using LabFusion.Entities;
using LabFusion.SDK.Extenders;
using LabFusion.Marrow.Integration;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Scene;

namespace LabFusion.Network;

public static class RPCStringSender
{
    public static bool SetValue(RPCString rpcString, string value) 
    {
        // Make sure we have a server
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return false;
        }

        // Check for ownership
        if (RPCVariableExtender.Cache.TryGet(rpcString, out var entity))
        {
            if (rpcString.RequiresOwnership && !entity.IsOwner)
            {
                return false;
            }
        }
        else if (rpcString.RequiresOwnership && !NetworkInfo.IsHost)
        {
            return false;
        }

        // Send the message
        var pathData = ComponentPathData.CreateFromComponent<RPCVariable, RPCVariableExtender>(rpcString, RPCVariable.HashTable, RPCVariableExtender.Cache);
        var stringData = RPCStringData.Create(pathData, value);

        MessageRelay.RelayNative(stringData, NativeMessageTag.RPCString, CommonMessageRoutes.ReliableToClients);

        return true;
    }

    public static void CatchupValue(RPCString rpcString, PlayerID playerID)
    {
        // Send the message
        var pathData = ComponentPathData.CreateFromComponent<RPCVariable, RPCVariableExtender>(rpcString, RPCVariable.HashTable, RPCVariableExtender.Cache);
        var boolData = RPCStringData.Create(pathData, rpcString.GetLatestValue());

        MessageRelay.RelayNative(boolData, NativeMessageTag.RPCString, new MessageRoute(playerID.SmallID, NetworkChannel.Reliable));
    }
}

public class RPCStringData : INetSerializable
{
    public ComponentPathData PathData;
    public string Value;

    public int? GetSize()
    {
        return ComponentPathData.Size + Value.GetSize();
    }

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PathData);
        serializer.SerializeValue(ref Value);
    }

    public static RPCStringData Create(ComponentPathData pathData, string value)
    {
        return new RPCStringData()
        {
            PathData = pathData,
            Value = value,
        };
    }
}

[Net.SkipHandleWhileLoading]
public class RPCStringMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.RPCString;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<RPCStringData>();

        if (data.PathData.TryGetComponent<RPCVariable, RPCVariableExtender>(RPCVariable.HashTable, out var rpcVariable))
        {
            var rpcString = rpcVariable.TryCast<RPCString>();

            if (rpcString == null)
            {
                return;
            }

            OnFoundRPCString(rpcString, data.Value);
        }
    }

    private static void OnFoundRPCString(RPCString rpcString, string value)
    {
        rpcString.ReceiveValue(value);
    }
}