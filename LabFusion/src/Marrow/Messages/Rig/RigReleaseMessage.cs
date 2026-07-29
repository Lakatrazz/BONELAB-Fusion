using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Entities;
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

public class RigReleaseMessage : ModuleMessageHandler
{
    protected override bool OnPreRelayMessage(ReceivedMessage received) => CommonMessageValidation.ValidateSenderOwnsEntity(received);

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
