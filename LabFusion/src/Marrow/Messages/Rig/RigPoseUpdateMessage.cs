using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabFusion.Network.Messages;

namespace LabFusion.Marrow.Messages;

public class RigPoseUpdateData : INetSerializable
{
    public const int Size = NetworkEntityReference.Size + RigPose.Size;

    public int? GetSize() => Size;

    public NetworkEntityReference RigReference;

    public RigPose Pose;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref RigReference);
        serializer.SerializeValue(ref Pose);
    }

    public static RigPoseUpdateData Create(RigPose pose)
    {
        var data = new RigPoseUpdateData
        {
            Pose = pose,
        };

        return data;
    }
}

[Net.SkipHandleWhileLoading]
public class RigPoseUpdateMessage : ModuleMessageHandler
{
    protected override bool OnPreRelayMessage(ReceivedMessage received) => CommonMessageValidation.ValidateSenderOwnsEntity(received);

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<RigPoseUpdateData>();

        var rigReference = data.RigReference;

        if (!rigReference.TryGetEntity(out var networkEntity))
        {
            return;
        }

        var networkRig = networkEntity.GetExtender<NetworkRig>();

        if (networkRig == null)
        {
            return;
        }

        var pose = data.Pose;

        if (!pose.IsValid())
        {
            return;
        }

        networkRig.OnPoseReceived(pose);
    }
}