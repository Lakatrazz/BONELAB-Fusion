using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Representation;
using LabFusion.Utilities;
using SLZ.Bonelab;
using SLZ.Marrow.SceneStreaming;
using SLZ.Marrow.Warehouse;
using System;

namespace LabFusion.Network
{
    public class SceneLoadData : IFusionSerializable, IDisposable
    {
        public string levelBarcode;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(levelBarcode);
        }

        public void Deserialize(FusionReader reader)
        {
            levelBarcode = reader.ReadString();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static SceneLoadData Create(string levelBarcode) {
            return new SceneLoadData() {
                levelBarcode = levelBarcode,
            };
        }
    }

    public class SceneLoadMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.SceneLoad;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            if (!NetworkInfo.IsServer && !isServerHandled) {
                using (var reader = FusionReader.Create(bytes)) {
                    using (var data = reader.ReadFusionSerializable<SceneLoadData>()) {
#if DEBUG
                        FusionLogger.Log($"Received level load for {data.levelBarcode}!");
#endif

                        FusionSceneManager.SetTargetScene(data.levelBarcode);
                    }
                }
            }
        }
    }
}
