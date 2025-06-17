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
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return false;
        }

        // Check for ownership
        if (RPCVariableExtender.Cache.TryGet(rpcInt, out var entity))
        {
            if (rpcInt.RequiresOwnership && !entity.IsOwner)
            {
                return false;
            }
        }
        else if (rpcInt.RequiresOwnership && !NetworkInfo.IsHost)
        {
            return false;
        }

        // Send the message
        var pathData = ComponentPathData.CreateFromComponent<RPCVariable, RPCVariableExtender>(rpcInt, RPCVariable.HashTable, RPCVariableExtender.Cache);
        var intData = RPCIntData.Create(pathData, value);

        MessageRelay.RelayNative(intData, NativeMessageTag.RPCInt, CommonMessageRoutes.ReliableToClients);

        return true;
    }

    public static void CatchupValue(RPCInt rpcInt, PlayerID playerID)
    {
        // Send the message
        var pathData = ComponentPathData.CreateFromComponent<RPCVariable, RPCVariableExtender>(rpcInt, RPCVariable.HashTable, RPCVariableExtender.Cache);
        var boolData = RPCIntData.Create(pathData, rpcInt.GetLatestValue());

        MessageRelay.RelayNative(boolData, NativeMessageTag.RPCInt, new MessageRoute(playerID.SmallID, NetworkChannel.Reliable));
    }
}

public class RPCIntData : INetSerializable
{
    public ComponentPathData PathData;
    public int Value;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PathData);
        serializer.SerializeValue(ref Value);
    }

    public static RPCIntData Create(ComponentPathData pathData, int value)
    {
        return new RPCIntData()
        {
            PathData = pathData,
            Value = value,
        };
    }
}

[Net.SkipHandleWhileLoading]
public class RPCIntMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.RPCInt;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<RPCIntData>();

        if (data.PathData.TryGetComponent<RPCVariable, RPCVariableExtender>(RPCVariable.HashTable, out var rpcVariable))
        {
            var rpcInt = rpcVariable.TryCast<RPCInt>();

            if (rpcInt == null)
            {
                return;
            }

            OnFoundRPCInt(rpcInt, data.Value);
        }
    }

    private static void OnFoundRPCInt(RPCInt rpcInt, int value)
    {
        rpcInt.ReceiveValue(value);
    }
}