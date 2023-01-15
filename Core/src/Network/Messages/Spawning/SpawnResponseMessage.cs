using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Representation;
using LabFusion.Utilities;

using System;
using System.Collections;

using SLZ.Marrow.Pool;

using UnityEngine;

using BoneLib.Nullables;

using LabFusion.Syncables;

using SLZ.Interaction;
using SLZ.Marrow.Warehouse;
using SLZ.Marrow.Data;

using MelonLoader;

using SLZ.Zones;
using SLZ.AI;

using LabFusion.Extensions;

using SLZ.Props.Weapons;
using SLZ;

using LabFusion.Exceptions;

namespace LabFusion.Network
{
    public class SpawnResponseData : IFusionSerializable, IDisposable
    {
        public byte owner;
        public string barcode;
        public ushort syncId;

        public SerializedTransform serializedTransform;

        public string spawnerPath;

        public Handedness hand;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(owner);
            writer.Write(barcode);
            writer.Write(syncId);
            writer.Write(serializedTransform);
            writer.Write(spawnerPath);
            writer.Write((byte)hand);
        }

        public void Deserialize(FusionReader reader)
        {
            owner = reader.ReadByte();
            barcode = reader.ReadString();
            syncId = reader.ReadUInt16();
            serializedTransform = reader.ReadFusionSerializable<SerializedTransform>();
            spawnerPath = reader.ReadString();
            hand = (Handedness)reader.ReadByte();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static SpawnResponseData Create(byte owner, string barcode, ushort syncId, SerializedTransform serializedTransform, ZoneSpawner spawner = null, Handedness hand = Handedness.UNDEFINED)
        {
            string path = "_";

            if (spawner != null) {
                path = spawner.gameObject.GetFullPath();
            }

            return new SpawnResponseData()
            {
                owner = owner,
                barcode = barcode,
                syncId = syncId,
                serializedTransform = serializedTransform,
                spawnerPath = path,
                hand = hand,
            };
        }
    }

    [Net.DelayWhileLoading]
    public class SpawnResponseMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.SpawnResponse;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            if (!isServerHandled)
            {
                using (var reader = FusionReader.Create(bytes))
                {
                    using (var data = reader.ReadFusionSerializable<SpawnResponseData>())
                    {
                        var crateRef = new SpawnableCrateReference(data.barcode);

                        var spawnable = new Spawnable()
                        {
                            crateRef = crateRef,
                            policyData = null
                        };

                        AssetSpawner.Register(spawnable);

                        byte owner = data.owner;
                        ushort syncId = data.syncId;
                        string path = data.spawnerPath;
                        var hand = data.hand;
                        
                        NullableMethodExtensions.PoolManager_Spawn(spawnable, data.serializedTransform.position, data.serializedTransform.rotation.Expand(), null,
                            true, null, (Action<GameObject>)((go) => { OnSpawnFinished(owner, syncId, go, path, hand); }), null);
                    }
                }
            }
            else
                throw new ExpectedClientException();
        }

        public static void OnSpawnFinished(byte owner, ushort syncId, GameObject go, string spawnerPath = "_", Handedness hand = Handedness.UNDEFINED) {
            ZoneSpawner spawner = null;
            if (spawnerPath != "_") {
                try {
                    // Try finding the zone spawner
                    var spawnerGo = GameObjectUtilities.GetGameObject(spawnerPath);
                    spawner = ZoneSpawner.Cache.Get(spawnerGo);

                    // Assign the parent
                    if (spawner != null && spawner.parentOverride != null) {
                        go.transform.parent = spawner.parentOverride.transform;
                    }
                } 
                catch (Exception e) {
#if DEBUG
                    FusionLogger.LogException("trying to get ZoneSpawner", e);
#endif
                }
            }

            if (PropSyncable.Cache.TryGet(go, out var syncable))
                SyncManager.RemoveSyncable(syncable);

            var poolee = AssetPoolee.Cache.Get(go);
            if (poolee == null)
                poolee = go.AddComponent<AssetPoolee>();

            try {
                AIBrain brain;
                if (spawner != null && (brain = go.GetComponent<AIBrain>())) {
                    spawner.InsertNPC(brain);
                }
            }
            catch (Exception e) {
#if DEBUG
                FusionLogger.LogException("adding spawned object to zone spawner", e);
#endif
            }

            if (!NetworkInfo.IsServer)
                PooleeUtilities.PermitSpawning(poolee);
            else {
                PooleeUtilities.AddToServer(poolee);
            }

            PropSyncable newSyncable = new PropSyncable(go.GetComponentInChildren<InteractableHost>(true), go.gameObject);
            newSyncable.SetOwner(owner);

            SyncManager.RegisterSyncable(newSyncable, syncId);

            // Force the object active
            Grip grip = null;
            go.SetActive(true);
            PooleeUtilities.KeepForceEnabled(poolee);

            // Setup magazine info
            var magazine = go.GetComponent<Magazine>();
            if (magazine) {
                grip = magazine.grip;

                var ammoInventory = RigData.RigReferences.RigManager.AmmoInventory;

                CartridgeData cart = ammoInventory.ammoReceiver.defaultLightCart;

                if (owner == PlayerIdManager.LocalSmallId) { 
                    if (ammoInventory.ammoReceiver._selectedCartridgeData != null)
                        cart = ammoInventory.ammoReceiver._selectedCartridgeData;
                }
                else if (PlayerRep.Representations.TryGetValue(owner, out var rep)) {
                    if (rep.RigReferences.RigManager.AmmoInventory.ammoReceiver._selectedCartridgeData != null) {
                        ammoInventory = rep.RigReferences.RigManager.AmmoInventory;

                        cart = ammoInventory.ammoReceiver._selectedCartridgeData;
                    }
                }

                magazine.Initialize(cart, ammoInventory.GetCartridgeCount(cart));
                magazine.Claim();

                NullableMethodExtensions.AudioPlayer_PlayAtPoint(ammoInventory.ammoReceiver.grabClips, ammoInventory.ammoReceiver.transform.position, null, null, false, null, null);

                // Attach the object to the hand
                if (owner == PlayerIdManager.LocalSmallId) {
                    var found = RigData.RigReferences.GetHand(hand);

                    if (found) {
                        found.GrabLock = false;

                        if (found.HasAttachedObject()) {
                            var current = Grip.Cache.Get(found.m_CurrentAttachedGO);
                            if (current)
                                current.ForceDetach(found);
                        }

                        MelonCoroutines.Start(Internal_ForceGrabConfirm(found, grip));
                    }
                }
                else if (PlayerRep.Representations.TryGetValue(owner, out var rep)) {
                    var hostTransform = grip.Host.GetTransform();
                    var repHand = rep.RigReferences.GetHand(hand);
                    hostTransform.position = repHand.transform.position;
                    hostTransform.rotation = repHand.transform.rotation;

                    rep.AttachObject(hand, grip);
                }
            }

            MelonCoroutines.Start(PostSpawnRoutine(poolee, owner, grip, hand));
        }

        private static IEnumerator Internal_ForceGrabConfirm(Hand hand, Grip grip) {
            yield return null;

            var hostTransform = grip.Host.GetTransform();
            hostTransform.position = hand.transform.position;
            hostTransform.rotation = hand.transform.rotation;

            grip.OnGrabConfirm(hand, true);
        }

        private static IEnumerator PostSpawnRoutine(AssetPoolee __instance, byte owner, Grip grip = null, Handedness hand = Handedness.UNDEFINED) {
            for (var i = 0; i < 3; i++) {
                yield return null;
            }

            PooleeUtilities.DequeueSpawning(__instance);
            PooleeUtilities.RemoveForceEnabled(__instance);
        }
    }
}
