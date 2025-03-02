using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class SpawnRequestData : INetSerializable
{
    public const int Size = sizeof(byte) * 2 + SerializedTransform.Size;

    public string barcode;
    public SerializedTransform serializedTransform;

    public uint trackerId;

    public bool spawnEffect;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref barcode);
        serializer.SerializeValue(ref serializedTransform);
        serializer.SerializeValue(ref trackerId);
        serializer.SerializeValue(ref spawnEffect);
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

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ServerOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<SpawnRequestData>();

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