using LabFusion.Data;
using LabFusion.Utilities;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Data;

using UnityEngine;

namespace LabFusion.RPC;

public static class NetworkAssetSpawner
{
    public struct SpawnCallbackInfo
    {
        public GameObject spawned;

        public NetworkEntity entity;
    }

    public struct SpawnRequestInfo
    {
        public Spawnable spawnable;

        public Vector3 position;

        public Quaternion rotation;

        public Action<SpawnCallbackInfo> spawnCallback;

        public bool spawnEffect;
    }

    private static uint _lastTrackedSpawnable = 0;
    
    private static readonly Dictionary<uint, Action<SpawnCallbackInfo>> _callbackQueue = new();

    public static void OnSpawnComplete(uint trackerId, SpawnCallbackInfo info)
    {
        if (_callbackQueue.TryGetValue(trackerId, out var callback))
        {
            callback(info);
            _callbackQueue.Remove(trackerId);
        }
    }

    public static void Spawn(SpawnRequestInfo info)
    {
        uint trackerId = _lastTrackedSpawnable++;

        if (info.spawnCallback != null)
        {
            _callbackQueue.Add(trackerId, info.spawnCallback);
        }

        PooleeUtilities.RequestSpawn(info.spawnable.crateRef.Barcode.ID, new SerializedTransform(info.position, info.rotation), trackerId, info.spawnEffect);
    }
}