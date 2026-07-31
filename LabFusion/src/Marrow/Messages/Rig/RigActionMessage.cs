using LabFusion.Entities;
using LabFusion.Marrow.Rig;
using LabFusion.Network;
using LabFusion.Network.Messages;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;

namespace LabFusion.Marrow.Messages;

public class RigActionData : INetSerializable
{
    public const int Size = NetworkEntityReference.Size + sizeof(byte);

    public NetworkEntityReference RigReference;

    public RigActionType Type;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref RigReference);
        serializer.SerializeValue(ref Type, Precision.OneByte);
    }
}

public class RigActionMessage : ModuleMessageHandler
{
    protected override bool OnPreRelayMessage(ReceivedMessage received) => CommonMessageValidation.ValidateSenderOwnsEntity(received);

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<RigActionData>();

        if (!NetworkBeingManager.TryGetNetworkRig(data.RigReference, out var networkRig))
        {
            return;
        }

        InvokeActionLocally(networkRig, data.Type);

        RigActionManager.OnRigAction(networkRig, data.Type);
    }

    private static void InvokeActionLocally(NetworkRig networkRig, RigActionType type)
    {
        if (!networkRig.HasRig)
        {
            return;
        }

        if (networkRig.NetworkEntity.IsOwner)
        {
            return;
        }

        var rigManager = networkRig.RigRefs.RigManager;
        var headSFX = rigManager.physicsRig.headSfx;

        switch (type)
        {
            case RigActionType.Jump:
                rigManager.remapHeptaRig.Jump();
                break;
            case RigActionType.Dying:
                headSFX.DyingVocal();
                break;
            case RigActionType.Death:
                headSFX.DeathVocal();
                networkRig.RigRefs.DisableInteraction();
                break;
            case RigActionType.Recovery:
                headSFX.RecoveryVocal();
                break;
            case RigActionType.Respawn:
                rigManager.health.Respawn();
                break;
        }
    }
}
