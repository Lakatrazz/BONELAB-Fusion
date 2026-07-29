using LabFusion.Entities;
using LabFusion.Marrow.Interaction;
using LabFusion.Network;
using LabFusion.Network.Messages;
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
    protected override bool OnPreRelayMessage(ReceivedMessage received) => CommonMessageValidation.ValidateSenderOwnsEntity(received);

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
