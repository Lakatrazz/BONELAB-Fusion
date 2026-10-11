using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Network;
using LabFusion.SDK.Modules;
using LabFusion.Network.Messages;
using LabFusion.Marrow.Data;

namespace LabFusion.Marrow.Messages;

public class RigBlendShapeData : INetSerializable
{
    public const int Size = NetworkEntityReference.Size + AvatarBlendShapePath.Size;

    public int? GetSize() => Size;

    public NetworkEntityReference RigReference;

    public AvatarBlendShapePath Path;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref RigReference);
        serializer.SerializeValue(ref Path);
    }
}

[Net.SkipHandleWhileLoading]
public class RigBlendShapeMessage : ModuleMessageHandler
{
    protected override bool OnPreRelayMessage(ReceivedMessage received) => CommonMessageValidation.ValidateSenderOwnsEntity(received);

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<RigBlendShapeData>();

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

        var path = data.Path;

        networkRig.BlendShapes.OnBlendShapeReceived(path);
    }
}