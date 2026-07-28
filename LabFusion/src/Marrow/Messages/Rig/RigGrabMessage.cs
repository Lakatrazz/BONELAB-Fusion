using LabFusion.Entities;
using LabFusion.Marrow.Interaction;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;

namespace LabFusion.Marrow.Messages;

public class RigGrabData : INetSerializable
{
    public NetworkEntityReference RigReference;

    public SerializedGrab Grab;

    public int? GetSize() => NetworkEntityReference.Size + Grab.GetSize();

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref RigReference);

        serializer.SerializeValue(ref Grab);
    }
}

public class RigGrabMessage : ModuleMessageHandler
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
        var data = received.ReadData<RigGrabData>();

        if (!data.RigReference.TryGetEntity(out var rigEntity))
        {
            return;
        }

        var rig = rigEntity.GetExtender<NetworkRig>();

        rig.RigGrabber.OnGrabReceived(data.Grab);
    }
}
