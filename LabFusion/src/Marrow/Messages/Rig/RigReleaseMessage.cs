using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Entities;
using LabFusion.Marrow.Rig;
using LabFusion.Network;
using LabFusion.Network.Messages;
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

[Net.SkipHandleWhileLoading]
public class RigReleaseMessage : ModuleMessageHandler
{
    protected override bool OnPreRelayMessage(ReceivedMessage received) => CommonMessageValidation.ValidateSenderOwnsEntity(received);

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<RigReleaseData>();

        if (!NetworkBeingManager.TryGetNetworkRig(data.RigReference, out var networkRig))
        {
            return;
        }

        if (!networkRig.HasRig)
        {
            return;
        }

        networkRig.RigGrabber.OnReleaseReceived(data.Handedness);
    }
}
