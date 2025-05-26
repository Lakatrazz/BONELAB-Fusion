using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Entities;
using LabFusion.Utilities;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class ConstraintDeleteData : INetSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort) * 2;

    public byte smallId;

    public ushort constraintId;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref smallId);
        serializer.SerializeValue(ref constraintId);
    }

    public static ConstraintDeleteData Create(byte smallId, ushort constraintId)
    {
        return new ConstraintDeleteData()
        {
            smallId = smallId,
            constraintId = constraintId,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class ConstraintDeleteMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.ConstraintDelete;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<ConstraintDeleteData>();

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.constraintId);

        if (entity == null)
        {
            return;
        }

        var networkConstraint = entity.GetExtender<NetworkConstraint>();

        if (networkConstraint == null)
        {
            return;
        }

        ConstraintTrackerPatches.IgnorePatches = true;
        networkConstraint.Tracker.DeleteConstraint();
        ConstraintTrackerPatches.IgnorePatches = false;

#if DEBUG
        FusionLogger.Log($"Unregistered constraint at ID {entity.ID}.");
#endif

        NetworkEntityManager.IdManager.UnregisterEntity(entity);
    }
}