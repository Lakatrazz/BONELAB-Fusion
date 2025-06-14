using Il2CppSLZ.Marrow;

using LabFusion.Entities;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public enum PhysicsRigStateType
{
    SHUTDOWN,
    RAGDOLL,
    LEG_SHUTDOWN,
    PHYSICAL_LEGS,
}

public class PhysicsRigStateData : INetSerializable
{
    public const int Size = sizeof(byte) * 4;

    public byte entityId;

    public PhysicsRigStateType type;
    public bool enabled;

    public bool left;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref entityId);
        serializer.SerializeValue(ref type, Precision.OneByte);
        serializer.SerializeValue(ref enabled);
        serializer.SerializeValue(ref left);
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
public class PlayerRepRagdollMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PhysicsRigState;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PhysicsRigStateData>();

        if (NetworkPlayerManager.TryGetPlayer(data.entityId, out var player))
        {
            player.EnqueuePhysicsRigState(data);
        }
    }
}