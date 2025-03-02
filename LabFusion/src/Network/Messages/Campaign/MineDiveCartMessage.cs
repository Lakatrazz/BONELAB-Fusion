using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class MineDiveCartData : INetSerializable
{
    public int amount;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref amount);
    }

    public static MineDiveCartData Create(int amount)
    {
        return new MineDiveCartData()
        {
            amount = amount,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class MineDiveCartMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.MineDiveCart;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<MineDiveCartData>();

        MineDiveData.CreateExtraCarts(data.amount);
    }
}
