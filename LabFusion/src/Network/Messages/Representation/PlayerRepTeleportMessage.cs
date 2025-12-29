using LabFusion.Extensions;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Scene;

using UnityEngine;

namespace LabFusion.Network;

public class PlayerRepTeleportData : INetSerializable
{
    public const int Size = sizeof(float) * 3;

    public Vector3 Position;

    public int? GetSize() => Size;

    public void Serialize(INetSerializer serializer)
    {
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
public class PlayerRepTeleportMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.PlayerRepTeleport;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerRepTeleportData>();

        // Teleport the player
        LocalPlayer.TeleportToPosition(data.Position, Vector3Extensions.forward);
    }
}