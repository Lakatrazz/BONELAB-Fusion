using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;

using SLZ;
using SLZ.Interaction;
using LabFusion.Syncables;
using LabFusion.Patching;

namespace LabFusion.Network
{
    public class InventorySlotInsertData : IFusionSerializable, IDisposable
    {
        public byte smallId;
        public byte inserter;
        public ushort syncId;
        public byte slotIndex;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(inserter);
            writer.Write(syncId);
            writer.Write(slotIndex);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            inserter = reader.ReadByte();
            syncId = reader.ReadUInt16();
            slotIndex = reader.ReadByte();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static InventorySlotInsertData Create(byte smallId, byte inserter, ushort syncId, byte slotIndex)
        {
            return new InventorySlotInsertData()
            {
                smallId = smallId,
                inserter = inserter,
                syncId = syncId,
                slotIndex = slotIndex,
            };
        }
    }

    [Net.DelayWhileLoading]
    public class InventorySlotInsertMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.InventorySlotInsert;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<InventorySlotInsertData>()) {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.inserter, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else {
                        if (SyncManager.TryGetSyncable(data.syncId, out var syncable) && syncable is PropSyncable propSyncable && propSyncable.TryGetExtender<WeaponSlotExtender>(out var extender)) {
                            RigReferenceCollection references = null;
                            
                            if (data.smallId == PlayerIdManager.LocalSmallId) {
                                references = RigData.RigReferences;
                            }
                            else if (PlayerRepManager.TryGetPlayerRep(data.smallId, out var rep)) {
                                references = rep.RigReferences;
                            }

                            if (references != null) {
                                InventorySlotReceiverDrop.PreventInsertCheck = true;
                                references.GetSlot(data.slotIndex).InsertInSlot(extender.Component.interactableHost);
                                InventorySlotReceiverDrop.PreventInsertCheck = false;
                            }
                        }
                    }
                }
            }
        }
    }
}
