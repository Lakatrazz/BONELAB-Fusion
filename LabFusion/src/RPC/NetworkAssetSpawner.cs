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
        public GameObject Spawned;

        public NetworkEntity Entity;
    }

    public struct SpawnRequestInfo
    {
        public Spawnable Spawnable;

        public Vector3 Position;

        public Quaternion Rotation;

        public Action<SpawnCallbackInfo> SpawnCallback;

        public bool SpawnEffect;
    }

    public struct DespawnRequestInfo
    {
        public ushort EntityID;

        public bool DespawnEffect;
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

        if (info.SpawnCallback != null)
        {
            _callbackQueue.Add(trackerId, info.SpawnCallback);
        }

        PooleeUtilities.RequestSpawn(info.Spawnable.crateRef.Barcode.ID, new SerializedTransform(info.Position, info.Rotation), trackerId, info.SpawnEffect);
    }

    public static void Despawn(DespawnRequestInfo info)
    {
        PooleeUtilities.RequestDespawn(info.EntityID, info.DespawnEffect);
    }
}