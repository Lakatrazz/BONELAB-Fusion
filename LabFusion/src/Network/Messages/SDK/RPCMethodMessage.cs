using LabFusion.Network.Serialization;
using LabFusion.RPC;

namespace LabFusion.Network;

public class RPCMethodData : INetSerializable
{
    public long MethodHash;

    public object[] Parameters;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref MethodHash);

        int length = 0;

        if (!serializer.IsReader)
        {
            length = Parameters.Length;
        }

        serializer.SerializeValue(ref length);

        if (serializer.IsReader)
        {
            Parameters = new object[length];
        }

        for (var i = 0; i <  length; i++)
        {
            var value = Parameters[i];

            serializer.SerializeValue(ref value);

            Parameters[i] = value;
        }
    }
}

public class RPCMethodMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.RPCMethod;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<RPCMethodData>();

        RpcManager.InvokeMethod(data);
    }
}