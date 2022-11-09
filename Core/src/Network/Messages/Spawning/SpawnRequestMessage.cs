using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Utilities;

using System;

namespace LabFusion.Network
{
    public class SpawnRequestData : IFusionSerializable, IDisposable
    {
        public byte owner;
        public string barcode;
        public SerializedTransform serializedTransform;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(owner);
            writer.Write(barcode);
            writer.Write(serializedTransform);
        }

        public void Deserialize(FusionReader reader)
        {
            owner = reader.ReadByte();
            barcode = reader.ReadString();
            serializedTransform = reader.ReadFusionSerializable<SerializedTransform>();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static SpawnRequestData Create(byte owner, string barcode, SerializedTransform serializedTransform)
        {
            return new SpawnRequestData()
            {
                owner = owner,
                barcode = barcode,
                serializedTransform = serializedTransform,
            };
        }
    }

    public class SpawnRequestMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.SpawnRequest;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            if (isServerHandled) {
                using (var reader = FusionReader.Create(bytes)) {
                    using (var data = reader.ReadFusionSerializable<SpawnRequestData>()) {
                        var syncId = SyncManager.AllocateSyncID();

                        PooleeUtilities.SendSpawn(data.owner, data.barcode, syncId, data.serializedTransform);
                    }
                }
            }
        }
    }
}
