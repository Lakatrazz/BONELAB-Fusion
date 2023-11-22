using System;
using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Utilities;

namespace LabFusion.Network
{
    public class SceneLoadData : IFusionSerializable, IDisposable
    {
        public string levelBarcode;

        public static int GetSize(string barcode)
        {
            return barcode.GetSize();
        }

        public void Serialize(FusionWriter writer)
        {
            writer.Write(levelBarcode);
        }

        public void Deserialize(FusionReader reader)
        {
            levelBarcode = reader.ReadString();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static SceneLoadData Create(string levelBarcode)
        {
            return new SceneLoadData
            {
                levelBarcode = levelBarcode,
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
                using var data = reader.ReadFusionSerializable<SceneLoadData>();
#if DEBUG
                FusionLogger.Log($"Received level load for {data.levelBarcode}!");
#endif

                FusionSceneManager.SetTargetScene(data.levelBarcode);
            }
        }
    }
}
