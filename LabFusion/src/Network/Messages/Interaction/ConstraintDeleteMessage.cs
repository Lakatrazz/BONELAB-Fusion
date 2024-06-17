using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Entities;

namespace LabFusion.Network;

public class ConstraintDeleteData : IFusionSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort) * 2;

    public byte smallId;

    public ushort constraintId;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(smallId);

        writer.Write(constraintId);
    }

    public void Deserialize(FusionReader reader)
    {
        smallId = reader.ReadByte();

        constraintId = reader.ReadUInt16();
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
public class ConstraintDeleteMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.ConstraintDelete;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<ConstraintDeleteData>();

        // Send message to all clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag.Value, bytes);
            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);

            return;
        }

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

        NetworkEntityManager.IdManager.UnregisterEntity(entity);
    }
}