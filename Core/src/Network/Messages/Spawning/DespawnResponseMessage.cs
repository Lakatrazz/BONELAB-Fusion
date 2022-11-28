using BoneLib.Nullables;
using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Utilities;
using SLZ.Player;
using System;

namespace LabFusion.Network
{
    public class DespawnResponseData : IFusionSerializable, IDisposable
    {
        public ushort syncId;
        public byte despawnerId;
        public bool isMag;

        public void Serialize(FusionWriter writer) {
            writer.Write(syncId);
            writer.Write(despawnerId);
            writer.Write(isMag);
        }

        public void Deserialize(FusionReader reader) {
            syncId = reader.ReadUInt16();
            despawnerId = reader.ReadByte();
            isMag = reader.ReadBoolean();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static DespawnResponseData Create(ushort syncId, byte despawnerId, bool isMag = false)
        {
            return new DespawnResponseData()
            {
                syncId = syncId,
                despawnerId = despawnerId,
                isMag = isMag,
            };
        }
    }

    [Net.DelayWhileLoading]
    public class DespawnResponseMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.DespawnResponse;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            // Despawn the poolee if it exists
            using (var reader = FusionReader.Create(bytes)) {
                using (var data = reader.ReadFusionSerializable<DespawnResponseData>()) {
                    if (SyncManager.TryGetSyncable(data.syncId, out var syncable) && syncable is PropSyncable propSyncable) {
                        PooleeUtilities.CanDespawn = true;

                        if (propSyncable.AssetPoolee && propSyncable.AssetPoolee.gameObject.activeInHierarchy) {
                            if (data.isMag) {
                                AmmoInventory ammoInventory = RigData.RigReferences.RigManager.AmmoInventory;

                                if (PlayerRep.Representations.TryGetValue(data.despawnerId, out var rep)) {
                                    ammoInventory = rep.RigReferences.RigManager.AmmoInventory;
                                }

                                NullableMethodExtensions.AudioPlayer_PlayAtPoint(ammoInventory.ammoReceiver.grabClips, ammoInventory.ammoReceiver.transform.position, null, null, false, null, null);

                                propSyncable.AssetPoolee.gameObject.SetActive(false);
                            }
                            else {
                                propSyncable.AssetPoolee.Despawn();
                            }
                        }

                        PooleeUtilities.CanDespawn = false;
                    }
                }
            }
        }
    }
}
