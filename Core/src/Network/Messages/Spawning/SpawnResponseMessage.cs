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
using static MelonLoader.MelonLogger;
using SLZ.Zones;
using SLZ.AI;
using LabFusion.Extensions;

namespace LabFusion.Network
{
    public class SpawnResponseData : IFusionSerializable, IDisposable
    {
        public byte owner;
        public string barcode;
        public ushort syncId;

        public SerializedTransform serializedTransform;

        public string spawnerPath;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(owner);
            writer.Write(barcode);
            writer.Write(syncId);
            writer.Write(serializedTransform);
            writer.Write(spawnerPath);
        }

        public void Deserialize(FusionReader reader)
        {
            owner = reader.ReadByte();
            barcode = reader.ReadString();
            syncId = reader.ReadUInt16();
            serializedTransform = reader.ReadFusionSerializable<SerializedTransform>();
            spawnerPath = reader.ReadString();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static SpawnResponseData Create(byte owner, string barcode, ushort syncId, SerializedTransform serializedTransform, ZoneSpawner spawner = null)
        {
            string path = "_";

            if (spawner != null) {
#if DEBUG
                FusionLogger.Log("Spawn Response spawner was not null! Getting path!");
#endif

                path = spawner.gameObject.GetFullPath();

#if DEBUG
                FusionLogger.Log($"ZoneSpawner {spawner.name} got a path!");
#endif
            }

            return new SpawnResponseData()
            {
                owner = owner,
                barcode = barcode,
                syncId = syncId,
                serializedTransform = serializedTransform,
                spawnerPath = path,
            };
        }
    }

    [Net.DelayWhileLoading]
    public class SpawnResponseMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.SpawnResponse;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            if (!isServerHandled) {
                using (var reader = FusionReader.Create(bytes)) {
                    using (var data = reader.ReadFusionSerializable<SpawnResponseData>()) {
                        var crateRef = new SpawnableCrateReference(data.barcode);

                        var spawnable = new Spawnable() {
                            crateRef = crateRef,
                            policyData = null
                        };

                        AssetSpawner.Register(spawnable);

                        byte owner = data.owner;
                        ushort syncId = data.syncId;
                        string path = data.spawnerPath;

                        NullableMethodExtensions.PoolManager_Spawn(spawnable, data.serializedTransform.position, data.serializedTransform.rotation.Expand(), null, 
                            true, null, (Action<GameObject>)((go) => { OnSpawnFinished(owner, syncId, go, path); }), null);
                    }
                }
            }
        }

        public static void OnSpawnFinished(byte owner, ushort syncId, GameObject go, string spawnerPath = "_") {
            ZoneSpawner spawner = null;
            if (spawnerPath != "_") {
                try {
                    var spawnerGo = GameObjectUtilities.GetGameObject(spawnerPath);
                    spawner = ZoneSpawner.Cache.Get(spawnerGo);
                } 
                catch { }
            }

            if (PropSyncable.Cache.TryGetValue(go, out var syncable))
                SyncManager.RemoveSyncable(syncable);

            var poolee = AssetPoolee.Cache.Get(go);
            if (poolee == null)
                poolee = go.AddComponent<AssetPoolee>();

            try {
                ZoneTracker tracker;
                if (spawner != null && (tracker = go.GetComponent<ZoneTracker>())) {
                    tracker.spawner = spawner;

                    AIBrain brain = go.GetComponent<AIBrain>();
                    if (!brain.IsNOC() && !brain.behaviour.IsNOC() && !spawner.currEnemyProfile.baseConfig.IsNOC()) {
                        brain.behaviour.SetBaseConfig(spawner.currEnemyProfile.baseConfig);
                    }
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

            go.SetActive(true);
            MelonCoroutines.Start(KeepEnabled(poolee));
        }
        
        public static IEnumerator KeepEnabled(AssetPoolee __instance) {
            for (var i = 0; i < 5; i++) {
                yield return null;

                __instance.gameObject.SetActive(true);
            }

            PooleeUtilities.DequeueSpawning(__instance);
        }
    }
}
