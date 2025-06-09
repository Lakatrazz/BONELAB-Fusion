using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Marrow;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Safety;
using LabFusion.Scene;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class SpawnRequestData : INetSerializable
{
    public int? GetSize() => Barcode.GetSize() + SerializedTransform.Size + sizeof(uint) + sizeof(bool);

    public string Barcode;
    public SerializedTransform SerializedTransform;

    public uint TrackerId;

    public bool SpawnEffect;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Barcode);
        serializer.SerializeValue(ref SerializedTransform);
        serializer.SerializeValue(ref TrackerId);
        serializer.SerializeValue(ref SpawnEffect);
    }

    public static SpawnRequestData Create(string barcode, SerializedTransform serializedTransform, uint trackerId, bool spawnEffect)
    {
        return new SpawnRequestData()
        {
            Barcode = barcode,
            SerializedTransform = serializedTransform,

            TrackerId = trackerId,

            SpawnEffect = spawnEffect,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class SpawnRequestMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.SpawnRequest;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ServerOnly;

    public const int MaxSpawnsPerSecond = 15;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<SpawnRequestData>();

        // Check for spawnable blacklist
        if (ModBlacklist.IsBlacklisted(data.Barcode) || GlobalModBlacklistManager.IsBarcodeBlacklisted(data.Barcode))
        {
#if DEBUG
            FusionLogger.Warn($"Blocking server spawn of spawnable {data.Barcode} because it is blacklisted!");
#endif

            return;
        }

        // Check for singleplayer only tag
        if (CrateFilterer.HasTags<SpawnableCrate>(new(data.Barcode), FusionTags.SingleplayerOnly))
        {
#if DEBUG
            FusionLogger.Warn($"Blocking server spawn of spawnable {data.Barcode} because it is tagged Singleplayer Only!");
#endif

            return;
        }

        // If the player isn't hosting a level, limit the amount of spawns per second
        if (!NetworkSceneManager.PlayerIsLevelHost(PlayerIDManager.GetPlayerID(received.Sender.Value)))
        {
            var activity = LimitedActivityManager.GetTracker(nameof(SpawnRequestMessage)).GetActivity(received.Sender.Value);

            activity.Increment();

            if (activity.Counter > MaxSpawnsPerSecond)
            {
                FusionLogger.Warn($"Blocking Player {received.Sender.Value}'s spawn of {data.Barcode} because they have tried to spawn {activity.Counter} entities in one second!");
                return;
            }
        }

        var entityId = NetworkEntityManager.IDManager.RegisteredEntities.AllocateNewId();

        PooleeUtilities.SendSpawn(received.Sender.Value, data.Barcode, entityId, data.SerializedTransform, data.TrackerId, data.SpawnEffect);
    }
}