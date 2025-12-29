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

    public PhysicsRigStateType Type;
    public bool Enabled;

    public bool Left;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Type, Precision.OneByte);
        serializer.SerializeValue(ref Enabled);
        serializer.SerializeValue(ref Left);
    }

    public void Apply(PhysicsRig physicsRig)
    {
        switch (Type)
        {
            case PhysicsRigStateType.SHUTDOWN:
                if (Enabled)
                {
                    physicsRig.ShutdownRig();
                }
                else
                {
                    physicsRig.TurnOnRig();
                }
                break;
            case PhysicsRigStateType.RAGDOLL:
                if (Enabled)
                {
                    physicsRig.RagdollRig();
                }
                else
                {
                    physicsRig.UnRagdollRig();
                }
                break;
            case PhysicsRigStateType.LEG_SHUTDOWN:
                var leg = Left ? physicsRig.legLf : physicsRig.legRt;

                if (Enabled)
                {
                    leg.ShutdownLimb();
                }
                break;
            case PhysicsRigStateType.PHYSICAL_LEGS:
                if (Enabled)
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

    public static PhysicsRigStateData Create(PhysicsRigStateType type, bool enabled, bool left = false)
    {
        return new PhysicsRigStateData
        {
            Type = type,
            Enabled = enabled,
            Left = left,
        };
    }
}

[Net.SkipHandleWhileLoading]
public class PhysicsRigStateMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PhysicsRigState;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var sender = received.Sender;

        if (!sender.HasValue)
        {
            return;
        }

        var data = received.ReadData<PhysicsRigStateData>();

        if (NetworkPlayerManager.TryGetPlayer(sender.Value, out var player))
        {
            player.EnqueuePhysicsRigState(data);
        }
    }
}