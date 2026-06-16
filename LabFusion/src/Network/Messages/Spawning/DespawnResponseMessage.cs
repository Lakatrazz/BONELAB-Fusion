using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class DespawnResponseData : INetSerializable
{
    public const int Size = PlayerReference.Size + NetworkEntityReference.Size + sizeof(bool);

    public PlayerReference Despawner;

    public NetworkEntityReference Entity;

    public bool DespawnEffect;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Despawner);
        serializer.SerializeValue(ref Entity);

        serializer.SerializeValue(ref DespawnEffect);
    }
}

[Net.DelayWhileTargetLoading]
public class DespawnResponseMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.DespawnResponse;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<DespawnResponseData>();

        if (!data.Entity.TryGetEntity(out var entity))
        {
            return;
        }

        // Only entities that implement despawning functionality can be despawned
        // This inherently accounts for players, as players should not implement despawning
        var despawnableExtender = entity.GetExtender<IEntityDespawnableExtender>();

        if (despawnableExtender == null)
        {
            return;
        }

#if DEBUG
        FusionLogger.Log($"Unregistering entity at ID {entity.ID} after despawning.");
#endif

        if (data.DespawnEffect)
        {
            despawnableExtender.PlayDespawnEffect();
        }

        despawnableExtender.OnDespawnReceived();

        NetworkEntityManager.IDManager.UnregisterEntity(entity);
    }
}