using LabFusion.SDK.Extenders;
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
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return false;
        }

        // Check for ownership
        if (RPCVariableExtender.Cache.TryGet(rpcFloat, out var entity))
        {
            if (rpcFloat.RequiresOwnership && !entity.IsOwner)
            {
                return false;
            }
        }
        else if (rpcFloat.RequiresOwnership && !NetworkInfo.IsHost)
        {
            return false;
        }

        // Send the message
        var pathData = ComponentPathData.CreateFromComponent<RPCVariable, RPCVariableExtender>(rpcFloat, RPCVariable.HashTable, RPCVariableExtender.Cache);
        var floatData = RPCFloatData.Create(pathData, value);

        MessageRelay.RelayNative(floatData, NativeMessageTag.RPCFloat, CommonMessageRoutes.ReliableToClients);

        return true;
    }

    public static void CatchupValue(RPCFloat rpcFloat, PlayerID playerID)
    {
        // Send the message
        var pathData = ComponentPathData.CreateFromComponent<RPCVariable, RPCVariableExtender>(rpcFloat, RPCVariable.HashTable, RPCVariableExtender.Cache);
        var boolData = RPCFloatData.Create(pathData, rpcFloat.GetLatestValue());

        MessageRelay.RelayNative(boolData, NativeMessageTag.RPCFloat, new MessageRoute(playerID.SmallID, NetworkChannel.Reliable));
    }
}

public class RPCFloatData : INetSerializable
{
    public ComponentPathData PathData;
    public float Value;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PathData);
        serializer.SerializeValue(ref Value);
    }

    public static RPCFloatData Create(ComponentPathData pathData, float value)
    {
        return new RPCFloatData()
        {
            PathData = pathData,
            Value = value,
        };
    }
}

[Net.SkipHandleWhileLoading]
public class RPCFloatMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.RPCFloat;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<RPCFloatData>();

        if (data.PathData.TryGetComponent<RPCVariable, RPCVariableExtender>(RPCVariable.HashTable, out var rpcVariable))
        {
            var rpcFloat = rpcVariable.TryCast<RPCFloat>();

            if (rpcFloat == null)
            {
                return;
            }

            OnFoundRPCFloat(rpcFloat, data.Value);
        }
    }

    private static void OnFoundRPCFloat(RPCFloat rpcFloat, float value)
    {
        rpcFloat.ReceiveValue(value);
    }
}