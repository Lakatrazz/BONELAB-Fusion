using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Utilities;
using LabFusion.Scene;

namespace LabFusion.Network;

public class SceneLoadData : IFusionSerializable
{
    public string levelBarcode;
    public string loadBarcode;

    public static int GetSize(string barcode, string loadBarcode)
    {
        return barcode.GetSize() + loadBarcode.GetSize();
    }

    public int? GetSize()
    {
        return levelBarcode.GetSize() + loadBarcode.GetSize();
    }

    public void Serialize(FusionWriter writer)
    {
        writer.Write(levelBarcode);
        writer.Write(loadBarcode);
    }

    public void Deserialize(FusionReader reader)
    {
        levelBarcode = reader.ReadString();
        loadBarcode = reader.ReadString();
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