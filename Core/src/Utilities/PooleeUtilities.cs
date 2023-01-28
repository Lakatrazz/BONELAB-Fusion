using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Utilities;

using SLZ.Marrow.Pool;
using SLZ.Zones;
using SLZ.Interaction;
using SLZ;
using SLZ.Combat;
using SLZ.VFX;
using SLZ.Bonelab;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using SLZ.Props.Weapons;
using SLZ.Rig;
using SLZ.Props;

namespace LabFusion.Utilities {
    public static class PooleeUtilities {
        internal static PooleePusher ForceEnabled = new PooleePusher();

        internal static PooleePusher CheckingForSpawn = new PooleePusher();

        internal static PooleePusher CanSpawnList = new PooleePusher();

        internal static bool CanDespawn = false;

        internal static PooleePusher ServerSpawnedList = new PooleePusher();

        public static void OnServerLocalSpawn(ushort syncId, GameObject go) {
            if (!NetworkInfo.IsServer)
                return;

            if (PropSyncable.Cache.TryGet(go, out var syncable))
                SyncManager.RemoveSyncable(syncable);

            if (!AssetPoolee.Cache.Get(go))
                go.AddComponent<AssetPoolee>();

            PropSyncable newSyncable = new PropSyncable(go.GetComponentInChildren<InteractableHost>(true), go.gameObject);
            newSyncable.SetOwner(0);

            SyncManager.RegisterSyncable(newSyncable, syncId);
        }

        public static bool IsPlayer(AssetPoolee poolee) {
            if (poolee.IsNOC())
                return false;

            return poolee.GetComponentInChildren<RigManager>(true);
        }

        public static void SendDespawn(ushort syncId) {
            // Send response
            if (NetworkInfo.IsServer) {
                using (var writer = FusionWriter.Create()) {
                    using (var data = DespawnResponseData.Create(syncId, PlayerIdManager.LocalSmallId)) {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.DespawnResponse, writer)) {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
            // Send request
            else {
                using (var writer = FusionWriter.Create())
                {
                    using (var data = DespawnRequestData.Create(syncId, PlayerIdManager.LocalSmallId))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.DespawnRequest, writer))
                        {
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        public static void RequestDespawn(ushort syncId, bool isMag = false) {
            using (var writer = FusionWriter.Create())
            {
                using (var data = DespawnRequestData.Create(syncId, PlayerIdManager.LocalSmallId, isMag))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.DespawnRequest, writer))
                    {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void RequestSpawn(string barcode, SerializedTransform serializedTransform, byte? owner = null, Handedness hand = Handedness.UNDEFINED) {
            using (var writer = FusionWriter.Create())
            {
                using (var data = SpawnRequestData.Create(owner.HasValue ? owner.Value : PlayerIdManager.LocalSmallId, barcode, serializedTransform, hand))
                {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.SpawnRequest, writer)) {
                        MessageSender.SendToServer(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static void SendSpawn(byte owner, string barcode, ushort syncId, SerializedTransform serializedTransform, bool ignoreSelf = false, ZoneSpawner spawner = null, Handedness hand = Handedness.UNDEFINED) {
            using (var writer = FusionWriter.Create()) {
                using (var data = SpawnResponseData.Create(owner, barcode, syncId, serializedTransform, spawner, hand)) {
                    writer.Write(data);

                    using (var message = FusionMessage.Create(NativeMessageTag.SpawnResponse, writer)) {
                        if (!ignoreSelf)
                            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                        else
                            MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
                    }
                }
            }
        }

        public static bool CanForceDespawn(AssetPoolee instance) {
            return !CanSpawnList.Pull(instance) && IsWhitelisted(instance);
        }

        public static bool CanSendSpawn(AssetPoolee instance) {
            return IsWhitelisted(instance);
        }

        private static bool IsWhitelisted(AssetPoolee instance) {
            bool hasRigidbody = instance.GetComponentInChildren<Rigidbody>(true) != null;

            bool hasGunProperties = instance.GetComponentInChildren<FirearmCartridge>(true) == null || instance.GetComponentInChildren<Gun>(true) != null;
            bool miscProperties = instance.GetComponentInChildren<GetVelocity>(true) == null && instance.GetComponentInChildren<SpawnFragment>(true) == null;
            bool projectileProperties = instance.GetComponentInChildren<ProjectileBalloon>(true) == null;
            bool spawnableProperties = instance.spawnableCrate.Barcode != SpawnableWarehouseUtilities.BOARD_BARCODE;

            bool isValid = hasRigidbody && hasGunProperties && miscProperties && projectileProperties && spawnableProperties;

            return isValid;
        }
    }
}
