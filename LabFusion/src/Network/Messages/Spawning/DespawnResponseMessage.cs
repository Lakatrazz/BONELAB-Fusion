using LabFusion.Utilities;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Marrow.Patching;

using Il2CppSLZ.Marrow.VFX;

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

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        // Despawn the poolee if it exists
        var data = received.ReadData<DespawnResponseData>();

        if (!data.Entity.TryGetEntity(out var entity))
        {
            return;
        }

        // Don't allow the despawning of players
        if (entity.GetExtender<NetworkPlayer>() != null)
        {
            return;
        }

        var pooleeExtender = entity.GetExtender<PooleeExtender>();

        if (pooleeExtender == null)
        {
            return;
        }

        PooleeDespawnPatch.IgnorePatch = true;

        var poolee = pooleeExtender.Component;

#if DEBUG
        FusionLogger.Log($"Unregistering entity at ID {entity.ID} after despawning.");
#endif

        var marrowEntity = entity.GetExtender<IMarrowEntityExtender>();

        if (marrowEntity != null && data.DespawnEffect)
        {
            SpawnEffects.CallDespawnEffect(marrowEntity.MarrowEntity);
        }

        poolee.Despawn();

        NetworkEntityManager.IDManager.UnregisterEntity(entity);

        PooleeDespawnPatch.IgnorePatch = false;
    }
}