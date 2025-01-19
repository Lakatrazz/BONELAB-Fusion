using Il2CppSLZ.Marrow;

using LabFusion.Data;
using LabFusion.Entities;

namespace LabFusion.Network;

public enum PhysicsRigStateType
{
    SHUTDOWN,
    RAGDOLL,
    LEG_SHUTDOWN,
    PHYSICAL_LEGS,
}

public class PhysicsRigStateData : IFusionSerializable
{
    public const int Size = sizeof(byte) * 4;

    public byte entityId;

    public PhysicsRigStateType type;
    public bool enabled;

    public bool left;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(entityId);

        writer.Write((byte)type);
        writer.Write(enabled);

        writer.Write(left);
    }

    public void Deserialize(FusionReader reader)
    {
        entityId = reader.ReadByte();

        type = (PhysicsRigStateType)reader.ReadByte();
        enabled = reader.ReadBoolean();

        left = reader.ReadBoolean();
    }

    public void Apply(PhysicsRig physicsRig)
    {
        switch (type)
        {
            case PhysicsRigStateType.SHUTDOWN:
                if (enabled)
                {
                    physicsRig.ShutdownRig();
                }
                else
                {
                    physicsRig.TurnOnRig();
                }
                break;
            case PhysicsRigStateType.RAGDOLL:
                if (enabled)
                {
                    physicsRig.RagdollRig();
                }
                else
                {
                    physicsRig.UnRagdollRig();
                }
                break;
            case PhysicsRigStateType.LEG_SHUTDOWN:
                var leg = left ? physicsRig.legLf : physicsRig.legRt;

                if (enabled)
                {
                    leg.ShutdownLimb();
                }
                break;
            case PhysicsRigStateType.PHYSICAL_LEGS:
                if (enabled)
                {
                    physicsRig.PhysicalLegs();
                }
                else
                {
                    physicsRig.KinematicLegs();
                }
                break;
        }
    }

    public static PhysicsRigStateData Create(byte entityId, PhysicsRigStateType type, bool enabled, bool left = false)
    {
        return new PhysicsRigStateData
        {
            entityId = entityId,
            type = type,
            enabled = enabled,
            left = left,
        };
    }
}

[Net.SkipHandleWhileLoading]
public class PlayerRepRagdollMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.PhysicsRigState;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using var reader = FusionReader.Create(bytes);

        var data = reader.ReadFusionSerializable<PhysicsRigStateData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag, bytes);
            MessageSender.BroadcastMessageExcept(data.entityId, NetworkChannel.Reliable, message, false);
            return;
        }

        if (NetworkPlayerManager.TryGetPlayer(data.entityId, out var player))
        {
            player.EnqueuePhysicsRigState(data);
        }
    }
}