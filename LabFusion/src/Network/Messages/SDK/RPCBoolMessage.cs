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
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return false;
        }

        // Check for ownership
        if (RPCVariableExtender.Cache.TryGet(rpcBool, out var entity))
        {
            if (rpcBool.RequiresOwnership && !entity.IsOwner)
            {
                return false;
            }
        }
        else if (rpcBool.RequiresOwnership && !NetworkInfo.IsHost)
        {
            return false;
        }

        // Send the message
        var pathData = ComponentPathData.CreateFromComponent<RPCVariable, RPCVariableExtender>(rpcBool, RPCVariable.HashTable, RPCVariableExtender.Cache);
        var boolData = RPCBoolData.Create(pathData, value);

        MessageRelay.RelayNative(boolData, NativeMessageTag.RPCBool, CommonMessageRoutes.ReliableToClients);

        return true;
    }

    public static void CatchupValue(RPCBool rpcBool, PlayerID playerID)
    {
        // Send the message
        var pathData = ComponentPathData.CreateFromComponent<RPCVariable, RPCVariableExtender>(rpcBool, RPCVariable.HashTable, RPCVariableExtender.Cache);
        var boolData = RPCBoolData.Create(pathData, rpcBool.GetLatestValue());

        MessageRelay.RelayNative(boolData, NativeMessageTag.RPCBool, new MessageRoute(playerID.SmallID, NetworkChannel.Reliable));
    }
}

[Net.SkipHandleWhileLoading]
public class RPCBoolData : INetSerializable
{
    public ComponentPathData PathData;
    public bool Value;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PathData);
        serializer.SerializeValue(ref Value);
    }

    public static RPCBoolData Create(ComponentPathData pathData, bool value)
    {
        return new RPCBoolData()
        {
            PathData = pathData,
            Value = value,
        };
    }
}


public class RPCBoolMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.RPCBool;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<RPCBoolData>();

        if (data.PathData.TryGetComponent<RPCVariable, RPCVariableExtender>(RPCVariable.HashTable, out var rpcVariable))
        {
            var rpcBool = rpcVariable.TryCast<RPCBool>();

            if (rpcBool == null)
            {
                return;
            }

            OnFoundRPCBool(rpcBool, data.Value);
        }
    }

    private static void OnFoundRPCBool(RPCBool rpcBool, bool value)
    {
        rpcBool.ReceiveValue(value);
    }
}