using LabFusion.Network.Serialization;
using LabFusion.Scene;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class LevelLoadData : INetSerializable
{
    public string LevelBarcode;

    public string LoadingScreenBarcode;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref LevelBarcode);
        serializer.SerializeValue(ref LoadingScreenBarcode);
    }

    public static LevelLoadData Create(string levelBarcode, string loadBarcode)
    {
        return new LevelLoadData()
        {
            LevelBarcode = levelBarcode,
            LoadingScreenBarcode = loadBarcode
        };
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
        FusionLogger.Log($"Received level load for {data.LevelBarcode}!");
#endif

        FusionSceneManager.SetTargetScene(data.LevelBarcode, data.LoadingScreenBarcode);
    }
}