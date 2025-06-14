using LabFusion.Extensions;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Scene;

using UnityEngine;

namespace LabFusion.Network;

public class PlayerRepTeleportData : INetSerializable
{
    public const int Size = sizeof(byte) + sizeof(float) * 3;

    public byte teleportedUser;
    public Vector3 position;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref teleportedUser);

        if (serializer.IsReader)
        {
            serializer.SerializeValue(ref position);
            position = NetworkTransformManager.DecodePosition(position);
        }
        else
        {
            var encodedPosition = NetworkTransformManager.EncodePosition(position);
            serializer.SerializeValue(ref encodedPosition);
        }
    }

    public static PlayerRepTeleportData Create(byte teleportedUser, Vector3 position)
    {
        return new PlayerRepTeleportData
        {
            teleportedUser = teleportedUser,
            position = position,
        };
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

        // Make sure this is us
        if (data.teleportedUser != PlayerIDManager.LocalSmallID)
        {
            return;
        }

        // Teleport the player
        LocalPlayer.TeleportToPosition(data.position, Vector3Extensions.forward);
    }
}