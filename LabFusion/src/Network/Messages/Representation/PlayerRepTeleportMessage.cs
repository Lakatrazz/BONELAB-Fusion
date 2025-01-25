using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Player;
using LabFusion.Scene;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Network;

public class PlayerRepTeleportData : IFusionSerializable
{
    public const int Size = sizeof(byte) + sizeof(float) * 3;

    public byte teleportedUser;
    public Vector3 position;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(teleportedUser);
        writer.Write(NetworkTransformManager.EncodePosition(position));
    }

    public void Deserialize(FusionReader reader)
    {
        teleportedUser = reader.ReadByte();
        position = NetworkTransformManager.DecodePosition(reader.ReadVector3());
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

    public override ExpectedType ExpectedReceiver => ExpectedType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<PlayerRepTeleportData>();

        // Make sure this is us
        if (data.teleportedUser != PlayerIdManager.LocalSmallId)
        {
            return;
        }

        // Teleport the player
        FusionPlayer.Teleport(data.position, Vector3Extensions.forward);
    }
}