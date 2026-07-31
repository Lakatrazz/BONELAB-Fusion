using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Scene;
using LabFusion.SDK.Modules;
using LabFusion.Network;
using LabFusion.Marrow.Rig;
using LabFusion.Marrow.Extensions;

using UnityEngine;

namespace LabFusion.Marrow.Messages;

public class RigTeleportData : INetSerializable
{
    public const int Size = NetworkEntityReference.Size + sizeof(float) * 3;

    public NetworkEntityReference RigReference;

    public Vector3 Position;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref RigReference);

        if (serializer.IsReader)
        {
            serializer.SerializeValue(ref Position);
            Position = NetworkTransformManager.DecodePosition(Position);
        }
        else
        {
            var encodedPosition = NetworkTransformManager.EncodePosition(Position);
            serializer.SerializeValue(ref encodedPosition);
        }
    }
}

[Net.SkipHandleWhileLoading]
public class RigTeleportMessage : ModuleMessageHandler
{
    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<RigTeleportData>();

        if (!NetworkBeingManager.TryGetNetworkRig(data.RigReference, out var networkRig))
        {
            return;
        }

        if (!networkRig.NetworkEntity.IsOwner)
        {
            return;
        }

        if (!networkRig.HasRig)
        {
            return;
        }

        networkRig.RigRefs.RigManager.TeleportToPosition(data.Position, true);
    }
}