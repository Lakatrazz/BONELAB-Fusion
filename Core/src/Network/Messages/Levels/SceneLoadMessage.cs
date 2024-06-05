using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Utilities;

namespace LabFusion.Network
{
    public class SceneLoadData : IFusionSerializable
    {
        public string levelBarcode;
        public string loadBarcode;

        public static int GetSize(string barcode, string loadBarcode)
        {
            return barcode.GetSize() + loadBarcode.GetSize();
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

    public class SceneLoadMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.SceneLoad;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            if (!NetworkInfo.IsServer && !isServerHandled)
            {
                using var reader = FusionReader.Create(bytes);
                var data = reader.ReadFusionSerializable<SceneLoadData>();
#if DEBUG
                FusionLogger.Log($"Received level load for {data.levelBarcode}!");
#endif

                FusionSceneManager.SetTargetScene(data.levelBarcode, data.loadBarcode);
            }
        }
    }
}
