using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Senders;
using LabFusion.RPC;
using LabFusion.Marrow;
using LabFusion.Entities;
using LabFusion.Downloading;
using LabFusion.Preferences.Client;
using LabFusion.Network.Serialization;
using LabFusion.Safety;
using LabFusion.Marrow.Pool;
using LabFusion.Marrow.Extenders;
using LabFusion.Marrow.Serialization;

using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow.Interaction;

using Il2CppSLZ.Marrow.VFX;

using UnityEngine;

namespace LabFusion.Network;

public class SpawnResponseData : INetSerializable
{
    public int? GetSize() => sizeof(byte) + sizeof(ushort) + SpawnData.GetSize();

    public byte OwnerID;

    public ushort EntityID;

    public SerializedSpawnData SpawnData;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref OwnerID);
        serializer.SerializeValue(ref EntityID);
        serializer.SerializeValue(ref SpawnData);
    }
}

[Net.DelayWhileTargetLoading]
public class SpawnResponseMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.SpawnResponse;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<SpawnResponseData>();

        var spawnData = data.SpawnData;

        byte owner = data.OwnerID;
        string barcode = spawnData.Barcode;
        ushort entityID = data.EntityID;
        var trackerID = spawnData.TrackerID;
        var spawnEffect = spawnData.SpawnEffect;

        // Check for spawnable blacklist
        if (ModBlacklist.IsBlacklisted(barcode) || GlobalModBlacklistManager.IsBarcodeBlacklisted(barcode))
        {
#if DEBUG
            FusionLogger.Warn($"Blocking client spawn of spawnable {data.SpawnData.Barcode} because it is blacklisted!");
#endif

            return;
        }

        bool hasCrate = CrateFilterer.HasCrate<SpawnableCrate>(new(barcode));

        if (!hasCrate)
        {
            bool shouldDownload = ClientSettings.Downloading.DownloadSpawnables.Value;

            // Check if we should download the mod (it's not blacklisted, mod downloading disabled, etc.)
            if (!shouldDownload)
            {
                return;
            }

            long maxBytes = DataConversions.ConvertMegabytesToBytes(ClientSettings.Downloading.MaxFileSize.Value);

            NetworkModRequester.RequestAndInstallMod(new NetworkModRequester.ModInstallInfo()
            { 
                Target = owner,
                Barcode = barcode,
                FinishDownloadCallback = OnModDownloaded,
                MaxBytes = maxBytes,
            });

            void OnModDownloaded(DownloadCallbackInfo info)
            {
                if (info.Result != ModResult.SUCCEEDED)
                {
                    FusionLogger.Warn($"Failed downloading spawnable {barcode}!");
                    return;
                }

                // In the future we'll replace an already existing "dummy" spawnable, so that if its been despawned we won't respawn it
                // This will also let us show previews while its downloading
                BeginSpawn();
            }

            return;
        }

        BeginSpawn();

        void BeginSpawn()
        {
            var spawnable = LocalAssetSpawner.CreateSpawnable(barcode);

            LocalAssetSpawner.Register(spawnable);

            void OnPooleeSpawned(Poolee poolee)
            {
                OnSpawnFinished(data, poolee);
            }

            LocalAssetSpawner.Spawn(spawnable, spawnData.SerializedTransform.position, spawnData.SerializedTransform.rotation, OnPooleeSpawned);
        }
    }

    public static void OnSpawnFinished(SpawnResponseData data, Poolee poolee)
    {
        // The poolee will never be null, so we don't have to check for it
        // Only case where it could be null is the object not spawning, but the spawn callback only executes when it exists
        var go = poolee.gameObject;

        // Remove the existing entity on this poolee if it exists
        if (PooleeExtender.Cache.TryGet(poolee, out var conflictingEntity))
        {
            FusionLogger.Warn($"Unregistered entity {conflictingEntity.ID} on poolee {poolee.name} due to conflicting id.");

            NetworkEntityManager.IDManager.UnregisterEntity(conflictingEntity);
        }

        NetworkEntity newEntity = null;

        // Get the marrow entity on the spawned object
        var marrowEntity = MarrowEntity.Cache.Get(go);

        // Make sure we have a marrow entity before creating a prop
        if (marrowEntity != null)
        {
            if (!SpawnableBlacklist.IsClientSide(data.SpawnData.Barcode))
            {
                newEntity = CreateNetworkEntity(go, data.SpawnData.Barcode, marrowEntity, data.OwnerID, data.EntityID, data.SpawnData.SpawnSource);
            }

            if (data.SpawnData.SpawnEffect)
            {
                SpawnEffects.CallSpawnEffect(marrowEntity);
            }
        }

        // Invoke spawn callback
        if (data.OwnerID == PlayerIDManager.LocalSmallID)
        {
            NetworkAssetSpawner.OnSpawnComplete(data.SpawnData.TrackerID, new NetworkAssetSpawner.SpawnCallbackInfo()
            {
                Spawned = go,
                Entity = newEntity,
            });
        }
    }

    private static NetworkEntity CreateNetworkEntity(GameObject gameObject, string barcode, MarrowEntity marrowEntity, byte ownerID, ushort entityID, EntitySource source)
    {
        // Create a network entity
        var playerID = PlayerIDManager.GetPlayerID(ownerID);

        NetworkEntity networkEntity = new()
        {
            Source = source,
        };
        networkEntity.SetOwner(playerID);

        // Setup a network prop
        NetworkProp newProp = new(networkEntity, marrowEntity);

        // Register this entity
        NetworkEntityManager.IDManager.RegisterEntity(entityID, networkEntity);

        // Insert the catchup hook for future users
        networkEntity.OnEntityCreationCatchup += (entity, player) =>
        {
            SpawnSender.SendCatchupSpawn(ownerID, barcode, entityID, new SerializedTransform(gameObject.transform), player, source);
        };

        CatchupManager.RequestEntityDataCatchup(new(networkEntity));

        return networkEntity;
    }
}