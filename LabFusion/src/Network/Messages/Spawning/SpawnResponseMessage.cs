using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.VFX;
using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Data;
using LabFusion.Downloading;
using LabFusion.Entities;
using LabFusion.Marrow;
using LabFusion.Marrow.Extenders;
using LabFusion.Marrow.Pool;
using LabFusion.Marrow.Serialization;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Preferences.Client;
using LabFusion.RPC;
using LabFusion.Safety;
using LabFusion.Senders;
using LabFusion.Utilities;

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

        NetworkEntity newNetworkEntity = null;
        NetworkPropGhost propGhost = null;

        if (!SpawnableBlacklist.IsClientSide(spawnData.Barcode))
        {
            newNetworkEntity = CreateGhostNetworkEntity(owner, entityID, spawnData.SpawnSource, out propGhost);

            InsertCatchupHook(newNetworkEntity, spawnData.Barcode, spawnData.SerializedTransform);
        }

        // Check for spawnable blacklist
        if (ModBlacklist.IsBlacklisted(barcode) || GlobalModBlacklistManager.IsBarcodeBlacklisted(barcode))
        {
#if DEBUG
            FusionLogger.Warn($"Blocking client spawn of spawnable {data.SpawnData.Barcode} because it is blacklisted!");
#endif

            return;
        }

        bool hasCrate = AssetWarehouseSearcher.HasCrate<SpawnableCrate>(new(barcode));

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
                Reporter = propGhost,
            });

            void OnModDownloaded(DownloadCallbackInfo info)
            {
                if (info.Result != ModResult.SUCCEEDED)
                {
                    propGhost?.OnDownloadFailed();

                    FusionLogger.Warn($"Failed downloading spawnable {barcode}!");
                    return;
                }

                BeginSpawn();
            }

            return;
        }

        BeginSpawn();

        void BeginSpawn()
        {
            // Check for singleplayer only tag
            if (AssetWarehouseSearcher.HasTags<SpawnableCrate>(new(barcode), FusionTags.SingleplayerOnly))
            {
#if DEBUG
                FusionLogger.Warn($"Blocking local spawn of spawnable {data.Barcode} because it is tagged Singleplayer Only!");
#endif

                return;
            }

            var spawnable = LocalAssetSpawner.CreateSpawnable(barcode);

            LocalAssetSpawner.Register(spawnable);

            void OnPooleeSpawned(Poolee poolee)
            {
                OnSpawnFinished(data, poolee, newNetworkEntity);
            }

            LocalAssetSpawner.Spawn(spawnable, spawnData.SerializedTransform.position, spawnData.SerializedTransform.rotation, OnPooleeSpawned);
        }
    }

    public static void OnSpawnFinished(SpawnResponseData data, Poolee poolee, NetworkEntity networkEntity)
    {
        // Clear the prop ghost, if it exists
        if (networkEntity != null)
        {
            ClearGhost(networkEntity);
        }

        // The poolee will never be null, so we don't have to check for it
        // Only case where it could be null is the object not spawning, but the spawn callback only executes when it exists
        var go = poolee.gameObject;

        // If the NetworkEntity was destroyed while the poolee was being spawned, then it can be despawned
        if (networkEntity != null && networkEntity.IsDestroyed)
        {
            poolee.Despawn();
            return;
        }

        // Update the use time of the poolee's crate
        PalletUseHistoryManager.MarkCrateUsed(poolee.SpawnableCrate);

        // Remove the existing entity on this poolee if it exists
        if (PooleeExtender.Cache.TryGet(poolee, out var conflictingEntity))
        {
            FusionLogger.Warn($"Unregistered entity {conflictingEntity.ID} on poolee {poolee.name} due to conflicting id.");

            NetworkEntityManager.IDManager.UnregisterEntity(conflictingEntity);
        }

        // Get the marrow entity on the spawned object
        var marrowEntity = MarrowEntity.Cache.Get(go);

        // Make sure we have a marrow entity before creating a prop
        if (marrowEntity != null)
        {
            if (networkEntity != null)
            {
                AttachProp(networkEntity, go, data.SpawnData.Barcode, marrowEntity);
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
                Entity = networkEntity,
            });
        }
    }

    private static NetworkEntity CreateGhostNetworkEntity(byte ownerID, ushort entityID, EntitySource source, out NetworkPropGhost propGhost)
    {
        // Create the NetworkEntity and assign its owner
        var playerID = PlayerIDManager.GetPlayerID(ownerID);

        NetworkEntity networkEntity = new()
        {
            Source = source,
        };
        networkEntity.SetOwner(playerID);

        NetworkEntityManager.IDManager.RegisterEntity(entityID, networkEntity);

        // Attach a prop ghost to the entity
        propGhost = new NetworkPropGhost(networkEntity);

        return networkEntity;
    }

    private static void ClearGhost(NetworkEntity networkEntity)
    {
        var propGhost = networkEntity.GetExtender<NetworkPropGhost>();

        if (propGhost != null)
        {
            networkEntity.DisconnectExtender(propGhost);
        }
    }

    private static void AttachProp(NetworkEntity networkEntity, GameObject gameObject, string barcode, MarrowEntity marrowEntity)
    {
        // Create the network prop
        var newProp = new NetworkProp(networkEntity, marrowEntity);

        CatchupManager.RequestEntityDataCatchup(new(networkEntity));
    }

    private static void InsertCatchupHook(NetworkEntity networkEntity, string barcode, SerializedTransform spawnTransform)
    {
        networkEntity.EntityCreationCatchingUp += (entity, player) =>
        {
            var transform = spawnTransform;

            var prop = entity.GetExtender<NetworkProp>();

            if (prop != null)
            {
                transform = new SerializedTransform(prop.MarrowEntity.transform);
            }

            var owner = networkEntity.OwnerID ?? PlayerIDManager.LocalID;

            SpawnSender.SendCatchupSpawn(owner.SmallID, barcode, networkEntity.ID, transform, player.SmallID, networkEntity.Source);
        };
    }
}