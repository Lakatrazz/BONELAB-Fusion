using LabFusion.Marrow.Serialization;
using LabFusion.Network.Serialization;
using LabFusion.Scene;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class LevelLoadData : INetSerializable
{
    public SerializedCrateReference LevelReference;

    public string LoadingScreenBarcode;

    public int? GetSize() => LevelReference.GetSize() + LoadingScreenBarcode.GetSize();

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref LevelReference);
        serializer.SerializeValue(ref LoadingScreenBarcode);
    }
}

public class LevelLoadMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.SceneLoad;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<LevelLoadData>();

#if DEBUG
        FusionLogger.Log($"Received level load for {data.LevelReference.Barcode}!");
#endif

        FusionSceneManager.SetTargetScene(data.LevelReference, data.LoadingScreenBarcode);
    }
}