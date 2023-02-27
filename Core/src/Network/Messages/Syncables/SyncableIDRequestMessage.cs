using LabFusion.Data;
using LabFusion.Utilities;
using LabFusion.Syncables;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network {
    public class SyncableIDRequestData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) + sizeof(ushort);

        public byte smallId;
        public ushort queuedId;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(queuedId);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            queuedId = reader.ReadUInt16();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static SyncableIDRequestData Create(byte smallId, ushort queuedId) {
            return new SyncableIDRequestData() {
                smallId = smallId,
                queuedId = queuedId
            };
        }
    }

    public class SyncableIDRequestMessage : FusionMessageHandler {
        public override byte? Tag => NativeMessageTag.SyncableIDRequest;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            if (NetworkInfo.IsServer && isServerHandled) {
                using (var reader = FusionReader.Create(bytes)) {
                    using (var data = reader.ReadFusionSerializable<SyncableIDRequestData>()) {

                        using (var writer = FusionWriter.Create(SyncableIDResponseData.Size)) {
                            using (var response = SyncableIDResponseData.Create(data.queuedId, SyncManager.AllocateSyncID())) {
                                writer.Write(response);

                                using (var message = FusionMessage.Create(NativeMessageTag.SyncableIDResponse, writer)) {
                                    MessageSender.SendFromServer(data.smallId, NetworkChannel.Reliable, message);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
