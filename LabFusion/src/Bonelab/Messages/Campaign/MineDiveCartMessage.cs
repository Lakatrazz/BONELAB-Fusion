using LabFusion.Bonelab.Scene;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;

namespace LabFusion.Bonelab.Messages;

public class MineDiveCartData : INetSerializable
{
    public int Amount;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Amount);
    }
}

[Net.DelayWhileTargetLoading]
public class MineDiveCartMessage : ModuleMessageHandler
{
    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<MineDiveCartData>();

        MineDiveEventHandler.CreateExtraCarts(data.Amount);
    }
}
