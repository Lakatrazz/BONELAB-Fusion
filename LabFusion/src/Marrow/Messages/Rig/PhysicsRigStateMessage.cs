using Il2CppSLZ.Marrow;

using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Network;
using LabFusion.SDK.Modules;

namespace LabFusion.Marrow.Messages;

public enum PhysicsRigStateType
{
    Shutdown,
    Ragdoll,
    LegShutdown,
    PhysicalLegs,
}

public class PhysicsRigStateData : INetSerializable
{
    public const int Size = NetworkEntityReference.Size + sizeof(byte) * 3;

    public NetworkEntityReference RigReference;

    public PhysicsRigStateType Type;
    public bool Enabled;

    public bool Left;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref RigReference);

        serializer.SerializeValue(ref Type, Precision.OneByte);
        serializer.SerializeValue(ref Enabled);
        serializer.SerializeValue(ref Left);
    }

    public void Apply(PhysicsRig physicsRig)
    {
        switch (Type)
        {
            case PhysicsRigStateType.Shutdown:
                if (Enabled)
                {
                    physicsRig.ShutdownRig();
                }
                else
                {
                    physicsRig.TurnOnRig();
                }
                break;
            case PhysicsRigStateType.Ragdoll:
                if (Enabled)
                {
                    physicsRig.RagdollRig();
                }
                else
                {
                    physicsRig.UnRagdollRig();
                }
                break;
            case PhysicsRigStateType.LegShutdown:
                var leg = Left ? physicsRig.legLf : physicsRig.legRt;

                if (Enabled)
                {
                    leg.ShutdownLimb();
                }
                break;
            case PhysicsRigStateType.PhysicalLegs:
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
}

[Net.SkipHandleWhileLoading]
public class PhysicsRigStateMessage : ModuleMessageHandler
{
    protected override bool OnPreRelayMessage(ReceivedMessage received)
    {
        // The NetworkEntityReference is the first thing written to the PhysicsRigStateData, so we can just read that
        var rigReference = received.ReadData<NetworkEntityReference>();

        // The sender should always be valid for this message, if not it should fail anyways
        var sender = received.Sender.Value;

        // The sender of the grab message should own that rig
        // If not, prevent the relaying of the message
        if (rigReference.TryGetEntity(out var rigEntity) && rigEntity.HasOwner && rigEntity.OwnerID.SmallID != sender)
        {
            return false;
        }

        return true;
    }

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PhysicsRigStateData>();

        if (!data.RigReference.TryGetEntity(out var networkEntity))
        {
            return;
        }

        var networkRig = networkEntity.GetExtender<NetworkRig>();

        if (networkRig == null)
        {
            return;
        }

        networkRig.EnqueuePhysicsRigState(data);
    }
}