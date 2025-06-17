using LabFusion.Entities;
using LabFusion.Marrow.Integration;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.SDK.Extenders;

using UnityEngine;

namespace LabFusion.Network;

public static class RPCVector3Sender
{
    public static bool SetValue(RPCVector3 rpcVector3, Vector3 value) 
    {
        // Make sure we have a server
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return false;
        }

        // Check for ownership
        if (RPCVariableExtender.Cache.TryGet(rpcVector3, out var entity))
        {
            if (rpcVector3.RequiresOwnership && !entity.IsOwner)
            {
                return false;
            }
        }
        else if (rpcVector3.RequiresOwnership && !NetworkInfo.IsHost)
        {
            return false;
        }

        // Send the message
        var pathData = ComponentPathData.CreateFromComponent<RPCVariable, RPCVariableExtender>(rpcVector3, RPCVariable.HashTable, RPCVariableExtender.Cache);
        var vector3Data = RPCVector3Data.Create(pathData, value);

        MessageRelay.RelayNative(vector3Data, NativeMessageTag.RPCVector3, CommonMessageRoutes.ReliableToClients);

        return true;
    }

    public static void CatchupValue(RPCVector3 rpcVector3, PlayerID playerID)
    {
        // Send the message
        var pathData = ComponentPathData.CreateFromComponent<RPCVariable, RPCVariableExtender>(rpcVector3, RPCVariable.HashTable, RPCVariableExtender.Cache);
        var boolData = RPCVector3Data.Create(pathData, rpcVector3.GetLatestValue());

        MessageRelay.RelayNative(boolData, NativeMessageTag.RPCVector3, new MessageRoute(playerID.SmallID, NetworkChannel.Reliable));
    }
}

public class RPCVector3Data : INetSerializable
{
    public ComponentPathData PathData;
    public Vector3 Value;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref PathData);
        serializer.SerializeValue(ref Value);
    }

    public static RPCVector3Data Create(ComponentPathData pathData, Vector3 value)
    {
        return new RPCVector3Data()
        {
            PathData = pathData,
            Value = value,
        };
    }
}

[Net.SkipHandleWhileLoading]
public class RPCVector3Message : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.RPCVector3;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<RPCVector3Data>();

        if (data.PathData.TryGetComponent<RPCVariable, RPCVariableExtender>(RPCVariable.HashTable, out var rpcVariable))
        {
            var rpcVector3 = rpcVariable.TryCast<RPCVector3>();

            if (rpcVector3 == null)
            {
                return;
            }

            OnFoundRPCVector3(rpcVector3, data.Value);
        }
    }

    private static void OnFoundRPCVector3(RPCVector3 rpcVector3, Vector3 value)
    {
        rpcVector3.ReceiveValue(value);
    }
}