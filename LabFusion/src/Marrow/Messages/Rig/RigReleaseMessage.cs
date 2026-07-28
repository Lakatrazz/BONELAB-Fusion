using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Entities;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;

namespace LabFusion.Marrow.Messages;

public class RigReleaseData : INetSerializable
{
    public NetworkEntityReference RigReference;

    public Handedness Handedness;

    public int? GetSize() => NetworkEntityReference.Size + sizeof(byte);

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref RigReference);

        serializer.SerializeValue(ref Handedness, Precision.OneByte);
    }
}

public class RigReleaseMessage : ModuleMessageHandler
{
    protected override bool OnPreRelayMessage(ReceivedMessage received)
    {
        // The NetworkEntityReference is the first thing written to the RigGrabData, so we can just read that
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
        var data = received.ReadData<RigReleaseData>();

        if (!data.RigReference.TryGetEntity(out var rigEntity))
        {
            return;
        }

        var rig = rigEntity.GetExtender<NetworkRig>();

        rig.RigGrabber.OnReleaseReceived(data.Handedness);
    }
}
