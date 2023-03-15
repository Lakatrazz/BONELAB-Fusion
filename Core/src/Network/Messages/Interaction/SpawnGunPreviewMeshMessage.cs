using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Syncables;

using SLZ.Marrow.Warehouse;

namespace LabFusion.Network
{
    public class SpawnGunPreviewMeshData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) + sizeof(ushort);

        public byte smallId;
        public ushort syncId;
        public string barcode;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(syncId);
            writer.Write(barcode);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            syncId = reader.ReadUInt16();
            barcode = reader.ReadString();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static SpawnGunPreviewMeshData Create(byte smallId, ushort syncId, string barcode)
        {
            return new SpawnGunPreviewMeshData()
            {
                smallId = smallId,
                syncId = syncId,
                barcode = barcode,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class SpawnGunPreviewMeshMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.SpawnGunPreviewMesh;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<SpawnGunPreviewMeshData>())
                {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else {
                        if (SyncManager.TryGetSyncable(data.syncId, out var syncable) && syncable is PropSyncable propSyncable && propSyncable.TryGetExtender<SpawnGunExtender>(out var extender)) {
                            var crateRef = new SpawnableCrateReference(data.barcode);

                            if (crateRef.Crate != null) {
                                SpawnGunPatches.IgnorePatches = true;

                                extender.Component._selectedCrate = crateRef.Crate;
                                extender.Component.SetPreviewMesh();

                                SpawnGunPatches.IgnorePatches = false;
                            }
                        }
                    }
                }
            }
        }
    }
}
