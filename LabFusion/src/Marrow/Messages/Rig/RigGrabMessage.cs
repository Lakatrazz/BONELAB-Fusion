using LabFusion.Entities;
using LabFusion.Marrow.Interaction;
using LabFusion.Marrow.Rig;
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

[Net.SkipHandleWhileLoading]
public class RigGrabMessage : ModuleMessageHandler
{
    protected override bool OnPreRelayMessage(ReceivedMessage received) => CommonMessageValidation.ValidateSenderOwnsEntity(received);

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<RigGrabData>();

        if (!NetworkBeingManager.TryGetNetworkRig(data.RigReference, out var networkRig))
        {
            return;
        }

        if (!networkRig.HasRig)
        {
            return;
        }

        networkRig.RigGrabber.OnGrabReceived(data.Grab);
    }
}
