using LabFusion.Network.Serialization;
using LabFusion.Scene;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class SceneLoadData : INetSerializable
{
    public string levelBarcode;
    public string loadBarcode;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref levelBarcode);
        serializer.SerializeValue(ref loadBarcode);
    }

    public static SceneLoadData Create(string levelBarcode, string loadBarcode)
    {
        return new SceneLoadData()
        {
            levelBarcode = levelBarcode,
            loadBarcode = loadBarcode
        };
    }
}

public class SceneLoadMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.SceneLoad;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<SceneLoadData>();

#if DEBUG
        FusionLogger.Log($"Received level load for {data.levelBarcode}!");
#endif

        FusionSceneManager.SetTargetScene(data.levelBarcode, data.loadBarcode);
    }
}