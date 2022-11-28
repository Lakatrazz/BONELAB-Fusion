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
        public ushort syncId;
        public byte slotIndex;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(syncId);
            writer.Write(slotIndex);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            syncId = reader.ReadUInt16();
            slotIndex = reader.ReadByte();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static InventorySlotInsertData Create(byte smallId, ushort syncId, byte slotIndex)
        {
            return new InventorySlotInsertData()
            {
                smallId = smallId,
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
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else {
                        if (PlayerRep.Representations.TryGetValue(data.smallId, out var rep) && SyncManager.TryGetSyncable(data.syncId, out var syncable) 
                            && syncable is PropSyncable propSyncable && propSyncable.WeaponSlot) {

                            InventorySlotReceiverDrop.PreventInsertCheck = true;
                            rep.RigReferences.GetSlot(data.slotIndex).InsertInSlot(propSyncable.WeaponSlot.interactableHost);
                            InventorySlotReceiverDrop.PreventInsertCheck = false;
                        }
                    }
                }
            }
        }
    }
}
