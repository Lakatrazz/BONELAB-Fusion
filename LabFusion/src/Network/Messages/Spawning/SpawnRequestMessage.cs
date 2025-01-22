using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Exceptions;
using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class SpawnRequestData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 2 + SerializedTransform.Size;

    public byte owner;
    public string barcode;
    public SerializedTransform serializedTransform;

    public uint trackerId;

    public bool spawnEffect;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(owner);
        writer.Write(barcode);
        writer.Write(serializedTransform);

        writer.Write(trackerId);

        writer.Write(spawnEffect);
    }

    public void Deserialize(FusionReader reader)
    {
        owner = reader.ReadByte();
        barcode = reader.ReadString();
        serializedTransform = reader.ReadFusionSerializable<SerializedTransform>();

        trackerId = reader.ReadUInt32();

        spawnEffect = reader.ReadBoolean();
    }

    public static SpawnRequestData Create(byte owner, string barcode, SerializedTransform serializedTransform, uint trackerId, bool spawnEffect)
    {
        return new SpawnRequestData()
        {
            owner = owner,
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

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        if (!isServerHandled)
        {
            throw new ExpectedServerException();
        }

        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<SpawnRequestData>();

        // Check for spawnable blacklist
        if (ModBlacklist.IsBlacklisted(data.barcode))
        {
#if DEBUG
            FusionLogger.Warn($"Blocking server spawn of spawnable {data.barcode} because it is blacklisted!");
#endif

            return;
        }

        var playerId = PlayerIdManager.GetPlayerId(data.owner);

        var entityId = NetworkEntityManager.IdManager.RegisteredEntities.AllocateNewId();

        PooleeUtilities.SendSpawn(data.owner, data.barcode, entityId, data.serializedTransform, data.trackerId, data.spawnEffect);
    }
}