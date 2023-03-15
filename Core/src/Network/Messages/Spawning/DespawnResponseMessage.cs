using BoneLib.Nullables;
using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Utilities;
using MelonLoader;
using SLZ.Player;
using System;
using System.Collections;

namespace LabFusion.Network
{
    public class DespawnResponseData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(ushort) + sizeof(byte) * 2;

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

    [Net.DelayWhileTargetLoading]
    public class DespawnResponseMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.DespawnResponse;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            // Despawn the poolee if it exists
            using (var reader = FusionReader.Create(bytes)) {
                using (var data = reader.ReadFusionSerializable<DespawnResponseData>()) {
                    MelonCoroutines.Start(Internal_WaitForValidDespawn(data.syncId, data.despawnerId, data.isMag));
                }
            }
        }

        private static IEnumerator Internal_WaitForValidDespawn(ushort syncId, byte despawnerId, bool isMag) {
            // Delay at most 300 frames until this syncable exists
            int i = 0;
            while (!SyncManager.HasSyncable(syncId)) {
                yield return null;

                i++;

                if (i >= 300)
                    break;
            }
            
            // Get the syncable from the valid id
            if (SyncManager.TryGetSyncable(syncId, out var syncable) && syncable is PropSyncable propSyncable)
            {
                PooleeUtilities.CanDespawn = true;

                if (propSyncable.AssetPoolee && propSyncable.AssetPoolee.gameObject.activeInHierarchy) {
                    if (isMag) {
                        AmmoInventory ammoInventory = RigData.RigReferences.RigManager.AmmoInventory;

                        if (PlayerRepManager.TryGetPlayerRep(despawnerId, out var rep)) {
                            ammoInventory = rep.RigReferences.RigManager.AmmoInventory;
                        }

                        NullableMethodExtensions.AudioPlayer_PlayAtPoint(ammoInventory.ammoReceiver.grabClips, ammoInventory.ammoReceiver.transform.position, null, null, false, null, null);

                        propSyncable.AssetPoolee.gameObject.SetActive(false);
                    }
                    else {
                        propSyncable.AssetPoolee.Despawn();
                    }
                }

                SyncManager.RemoveSyncable(syncable);

                PooleeUtilities.CanDespawn = false;
            }
        }
    }
}
