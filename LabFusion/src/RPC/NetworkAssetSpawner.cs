using Il2CppSLZ.Marrow.Data;

using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Marrow.Serialization;
using LabFusion.Network;

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

        public EntitySource SpawnSource;
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
        uint trackerID = _lastTrackedSpawnable++;

        if (info.SpawnCallback != null)
        {
            _callbackQueue.Add(trackerID, info.SpawnCallback);
        }

        var data = new SerializedSpawnData()
        {
            Barcode = info.Spawnable.crateRef.Barcode.ID,
            SerializedTransform = new SerializedTransform(info.Position, info.Rotation),
            SpawnEffect = info.SpawnEffect,
            TrackerID = trackerID,
            SpawnSource = info.SpawnSource,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.SpawnRequest, CommonMessageRoutes.ReliableToServer);
    }

    public static void Despawn(DespawnRequestInfo info)
    {
        var data = new DespawnRequestData()
        {
            Entity = new NetworkEntityReference(info.EntityID),
            DespawnEffect = info.DespawnEffect,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.DespawnRequest, CommonMessageRoutes.ReliableToServer);
    }
}