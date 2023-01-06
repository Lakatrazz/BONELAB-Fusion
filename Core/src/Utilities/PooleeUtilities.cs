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
        internal static List<AssetPoolee> ForceEnabled = new List<AssetPoolee>();

        internal static List<AssetPoolee> CanSpawnList = new List<AssetPoolee>();

        internal static bool CanDespawn = false;

        internal static List<AssetPoolee> ServerSpawnedList = new List<AssetPoolee>();

        public static void OnServerLocalSpawn(ushort syncId, GameObject go) {
            if (!NetworkInfo.IsServer)
                return;

            if (PropSyncable.Cache.TryGet(go, out var syncable))
                SyncManager.RemoveSyncable(syncable);

            var poolee = AssetPoolee.Cache.Get(go);
            if (poolee == null)
                poolee = go.AddComponent<AssetPoolee>();

            PropSyncable newSyncable = new PropSyncable(go.GetComponentInChildren<InteractableHost>(true), go.gameObject);
            newSyncable.SetOwner(0);

            SyncManager.RegisterSyncable(newSyncable, syncId);
        }

        public static void KeepForceEnabled(AssetPoolee poolee)
        {
            if (!ForceEnabled.Has(poolee))
                ForceEnabled.Add(poolee);
        }

        public static bool IsPlayer(AssetPoolee poolee) {
            return poolee.GetComponentInChildren<RigManager>(true);
        }

        public static bool IsForceEnabled(AssetPoolee poolee)
        {
            for (var i = 0; i < ForceEnabled.Count; i++) {
                var found = ForceEnabled[i];

                if (found == poolee) {
                    return true;
                }
            }

            return false;
        }

        public static void RemoveForceEnabled(AssetPoolee poolee)
        {
            for (var i = 0; i < ForceEnabled.Count; i++)
            {
                var found = ForceEnabled[i];

                if (found == poolee)
                {
                    ForceEnabled.RemoveAt(i);
                    return;
                }
            }
        }

        public static void AddToServer(AssetPoolee poolee) {
            if (!ServerSpawnedList.Has(poolee))
                ServerSpawnedList.Add(poolee);
        }

        public static bool DequeueServerSpawned(AssetPoolee poolee)
        {
            for (var i = 0; i < ServerSpawnedList.Count; i++) {
                var found = ServerSpawnedList[i];

                if (found == poolee) {
                    ServerSpawnedList.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public static void PermitSpawning(AssetPoolee poolee) {
            if (!CanSpawnList.Has(poolee))
                CanSpawnList.Add(poolee);
        }

        public static bool CanSpawn(AssetPoolee poolee) {
            return CanSpawnList.Has(poolee);
        }

        public static bool DequeueSpawning(AssetPoolee poolee) {
            for (var i = 0; i < CanSpawnList.Count; i++) {
                var found = CanSpawnList[i];

                if (found == poolee) {
                    CanSpawnList.RemoveAt(i);
                    return true;
                }
            }

            return false;
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
            return !DequeueSpawning(instance) && IsWhitelisted(instance);
        }

        public static bool CanSendSpawn(AssetPoolee instance) {
            return IsWhitelisted(instance);
        }

        private static bool IsWhitelisted(AssetPoolee instance) {
            bool hasRigidbody = instance.GetComponentInChildren<Rigidbody>(true) != null;

            bool hasGunProperties = instance.GetComponentInChildren<FirearmCartridge>(true) == null || instance.GetComponentInChildren<Gun>(true) != null;
            bool miscProperties = instance.GetComponentInChildren<GetVelocity>(true) == null && instance.GetComponentInChildren<SpawnFragment>(true) == null; 

            bool isValid = hasRigidbody && hasGunProperties && miscProperties;

            return isValid;
        }
    }
}
