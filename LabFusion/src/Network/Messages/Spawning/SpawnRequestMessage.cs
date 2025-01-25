using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class SpawnRequestData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 2 + SerializedTransform.Size;

    public string barcode;
    public SerializedTransform serializedTransform;

    public uint trackerId;

    public bool spawnEffect;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(barcode);
        writer.Write(serializedTransform);

        writer.Write(trackerId);

        writer.Write(spawnEffect);
    }

    public void Deserialize(FusionReader reader)
    {
        barcode = reader.ReadString();
        serializedTransform = reader.ReadFusionSerializable<SerializedTransform>();

        trackerId = reader.ReadUInt32();

        spawnEffect = reader.ReadBoolean();
    }

    public static SpawnRequestData Create(string barcode, SerializedTransform serializedTransform, uint trackerId, bool spawnEffect)
    {
        return new SpawnRequestData()
        {
            barcode = barcode,
            serializedTransform = serializedTransform,

            trackerId = trackerId,

            spawnEffect = spawnEffect,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class SpawnRequestMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.SpawnRequest;

    public override ExpectedType ExpectedReceiver => ExpectedType.ServerOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        using var reader = FusionReader.Create(received.Bytes);
        var data = reader.ReadFusionSerializable<SpawnRequestData>();

        // Check for spawnable blacklist
        if (ModBlacklist.IsBlacklisted(data.barcode))
        {
#if DEBUG
            FusionLogger.Warn($"Blocking server spawn of spawnable {data.barcode} because it is blacklisted!");
#endif

            return;
        }

        var entityId = NetworkEntityManager.IdManager.RegisteredEntities.AllocateNewId();

        PooleeUtilities.SendSpawn(received.Sender.Value, data.barcode, entityId, data.serializedTransform, data.trackerId, data.spawnEffect);
    }
}