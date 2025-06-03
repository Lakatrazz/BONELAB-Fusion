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

using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Interaction;

using Il2CppSLZ.Marrow.VFX;

namespace LabFusion.Network;

public class SpawnResponseData : INetSerializable
{
    public int? GetSize() => sizeof(byte) + Barcode.GetSize() + sizeof(ushort) + SerializedTransform.Size + sizeof(uint) + sizeof(bool);

    public byte OwnerID;
    public string Barcode;
    public ushort EntityID;

    public SerializedTransform SerializedTransform;

    public uint TrackerId;

    public bool SpawnEffect;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref OwnerID);
        serializer.SerializeValue(ref Barcode);
        serializer.SerializeValue(ref EntityID);
        serializer.SerializeValue(ref SerializedTransform);

        serializer.SerializeValue(ref TrackerId);

        serializer.SerializeValue(ref SpawnEffect);
    }

    public static SpawnResponseData Create(byte owner, string barcode, ushort entityId, SerializedTransform serializedTransform, uint trackerId = 0, bool spawnEffect = false)
    {
        return new SpawnResponseData()
        {
            OwnerID = owner,
            Barcode = barcode,
            EntityID = entityId,
            SerializedTransform = serializedTransform,
            TrackerId = trackerId,
            SpawnEffect = spawnEffect,
        };
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

        byte owner = data.OwnerID;
        string barcode = data.Barcode;
        ushort entityId = data.EntityID;
        var trackerId = data.TrackerId;
        var spawnEffect = data.SpawnEffect;

        // Check for spawnable blacklist
        if (ModBlacklist.IsBlacklisted(barcode) || GlobalModBlacklistManager.IsBarcodeBlacklisted(barcode))
        {
#if DEBUG
            FusionLogger.Warn($"Blocking client spawn of spawnable {data.Barcode} because it is blacklisted!");
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
                target = owner,
                barcode = barcode,
                finishDownloadCallback = OnModDownloaded,
                maxBytes = maxBytes,
            });

            void OnModDownloaded(DownloadCallbackInfo info)
            {
                if (info.result != ModResult.SUCCEEDED)
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
            var crateRef = new SpawnableCrateReference(barcode);

            var spawnable = new Spawnable()
            {
                crateRef = crateRef,
                policyData = null
            };

            AssetSpawner.Register(spawnable);

            void OnPooleeSpawned(Poolee go)
            {
                OnSpawnFinished(owner, barcode, entityId, go, trackerId, spawnEffect);
            }

            SafeAssetSpawner.Spawn(spawnable, data.SerializedTransform.position, data.SerializedTransform.rotation, OnPooleeSpawned);
        }
    }

    public static void OnSpawnFinished(byte owner, string barcode, ushort entityId, Poolee poolee, uint trackerId = 0, bool spawnEffect = false)
    {
        // The poolee will never be null, so we don't have to check for it
        // Only case where it could be null is the object not spawning, but the spawn callback only executes when it exists
        var go = poolee.gameObject;

        // Remove the existing entity on this poolee if it exists
        if (PooleeExtender.Cache.TryGet(poolee, out var conflictingEntity))
        {
            FusionLogger.Warn($"Unregistered entity {conflictingEntity.ID} on poolee {poolee.name} due to conflicting id.");

            NetworkEntityManager.IdManager.UnregisterEntity(conflictingEntity);
        }

        NetworkEntity newEntity = null;

        // Get the marrow entity on the spawned object
        var marrowEntity = MarrowEntity.Cache.Get(go);

        // Make sure we have a marrow entity before creating a prop
        if (marrowEntity != null)
        {
            // Create a network entity
            var ownerId = PlayerIDManager.GetPlayerID(owner);

            newEntity = new();
            newEntity.SetOwner(ownerId);

            // Setup a network prop
            NetworkProp newProp = new(newEntity, marrowEntity);

            // Register this entity
            NetworkEntityManager.IdManager.RegisterEntity(entityId, newEntity);

            // Insert the catchup hook for future users
            newEntity.OnEntityCreationCatchup += (entity, player) =>
            {
                SpawnSender.SendCatchupSpawn(owner, barcode, entityId, new SerializedTransform(go.transform), player);
            };

            if (spawnEffect)
            {
                SpawnEffects.CallSpawnEffect(marrowEntity);
            }

            CatchupManager.RequestEntityDataCatchup(new(newEntity));
        }

        // Invoke spawn callback
        if (owner == PlayerIDManager.LocalSmallID)
        {
            NetworkAssetSpawner.OnSpawnComplete(trackerId, new NetworkAssetSpawner.SpawnCallbackInfo()
            {
                Spawned = go,
                Entity = newEntity,
            });
        }
    }
}