using LabFusion.Data;
using LabFusion.Exceptions;

namespace LabFusion.Network;

public class MineDiveCartData : IFusionSerializable
{
    public int amount;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(amount);
    }

    public void Deserialize(FusionReader reader)
    {
        amount = reader.ReadInt32();
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
